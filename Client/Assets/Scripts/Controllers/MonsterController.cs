using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MonsterController : CreatureController
{
    private Coroutine _coPatrol;
    private Vector3Int _destCellPos;
    
    public override CreatureState State
    {
        get => _state;
        set
        {
            if(_state == value)
                return;

            base.State = value;
            
            if (_coPatrol != null)
            {
                StopCoroutine(_coPatrol);
                _coPatrol = null;
            }
        }
    }
    
    protected override void Init()
    {
        base.Init();
        State = CreatureState.Idle;
        Dir = MoveDir.None;
    }

    protected override void UpdateIdle()
    {
        base.UpdateIdle();

        if (_coPatrol == null)
        {
            _coPatrol = StartCoroutine("CoPatrol");
        }
    }

    protected override void MoveToNextPosition()
    {
        // 길찾기 ex) Astart
        Vector3Int moveCellDir = _destCellPos - CellPos;
        if (moveCellDir.x > 0)
            Dir = MoveDir.Right;
        else if (moveCellDir.x < 0)
            Dir = MoveDir.Left;
        else if (moveCellDir.y > 0)
            Dir = MoveDir.Up;
        else if (moveCellDir.y < 0)
            Dir = MoveDir.Down;
        else
            Dir = MoveDir.None;
        
        Vector3Int destPos = CellPos; 
        switch (_dir)
        {
            case MoveDir.Up:
                destPos += Vector3Int.up;
                break;
            
            case MoveDir.Down:
                destPos += Vector3Int.down;
                break;
            
            case MoveDir.Left:
                destPos += Vector3Int.left;
                break;
            
            case MoveDir.Right:
                destPos += Vector3Int.right;
                break;
        }
        
        if (Managers.Map.CanGo(destPos) && Managers.Obj.Find(destPos) == null)
        {
            CellPos = destPos;
        }
        else
        {
            State = CreatureState.Idle;
        }
    }

    public override void OnDamaged()
    {
        base.OnDamaged();

        // 이펙트 생성
        GameObject effect = Managers.Resource.Instantiate("Effect/DieEffect");
        effect.transform.position = transform.position;
        effect.GetComponent<Animator>().Play("start");
        GameObject.Destroy(effect, 0.5f); // 이펙트 소멸
                    
        // 몬스터 삭제
        Managers.Obj.Remove(gameObject);
        Managers.Resource.Destroy(gameObject);
    }

    IEnumerator CoPatrol()
    {
        int waitSecends = Random.Range(1, 4);
        yield return new WaitForSeconds(waitSecends);

        // 그냥 몇번만 트라이 하도록 지정
        for (int i = 0; i < 10; i++)
        {
            int xRange = Random.Range(-5, 6);
            int yRange = Random.Range(-5, 6);

            Vector3Int randPos = CellPos + new Vector3Int(xRange, yRange, 0);

            // 해당 좌표로 이동가능하고 (지형적 방해로)
            // 해당 위치에 오브젝트가 없다면 (오브젝트/몬스터)
            if (Managers.Map.CanGo(randPos) && Managers.Obj.Find(randPos) == null)
            {
                _destCellPos = randPos;
                State = CreatureState.Moving;
                
                // 코루틴 종료
                yield break;
            }
        }

        // 10번 시도해도 못찾으면 그냥 아이들로
        State = CreatureState.Idle;
    }
}

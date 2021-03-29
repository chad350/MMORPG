using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MonsterController : CreatureController
{
    [SerializeField]
    private Coroutine _coPatrol;
    [SerializeField]
    private Coroutine _coSearch;
    [SerializeField]
    private Vector3Int _destCellPos;

    [SerializeField]
    private GameObject _target;

    private float _searchRange = 5.0f;
    
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
            
            if (_coSearch != null)
            {
                StopCoroutine(_coSearch);
                _coSearch = null;
            }
        }
    }
    
    protected override void Init()
    {
        base.Init();
        State = CreatureState.Idle;
        Dir = MoveDir.None;

        _speed = 3;
    }

    protected override void UpdateIdle()
    {
        base.UpdateIdle();

        if (_coPatrol == null)
        {
            _coPatrol = StartCoroutine("CoPatrol");
        }
        
        if (_coSearch == null)
        {
            _coSearch = StartCoroutine("CoSearch");
        }
    }

    protected override void MoveToNextPosition()
    {
        Vector3Int destPos = _destCellPos;
        if (_target != null)
        {
            destPos = _target.GetComponent<CreatureController>().CellPos;
        }

        List<Vector3Int> path = Managers.Map.FindPath(CellPos, destPos,  true);
        // 길을 못찾은 경우, 타겟이 있는 경유, 경로가 너무 먼 경우
        if (path.Count < 2 || (_target != null && path.Count > 10)) 
        {
            _target = null;
            State = CreatureState.Idle;
            return;
        }
        
        // 0 은 현재 자기가 있는 위치
        Vector3Int nextPos = path[1];
        // 길찾기
        Vector3Int moveCellDir = nextPos - CellPos;
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
        
        
        if (Managers.Map.CanGo(nextPos) && Managers.Obj.Find(nextPos) == null)
        {
            CellPos = nextPos;
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
    
    IEnumerator CoSearch()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            
            // 이미 타겟을 찾은 상태 - 패스
            if(_target != null)
                continue;

            _target = Managers.Obj.Find((go) =>
            {
                PlayerController pc = go.GetComponent<PlayerController>();
                if (pc == null)
                    return false;

                Vector3Int dir = (pc.CellPos - CellPos);
                if (dir.magnitude > _searchRange)
                    return false;

                return true;
            });
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MonsterController : CreatureController
{
    private Coroutine _coSkill;
    private Coroutine _coPatrol;
    private Coroutine _coSearch;

    private Vector3Int _destCellPos;

    
    private GameObject _target;

    private float _searchRange = 10.0f;
    
    private float _skillRange = 1.0f;

    
    private bool _rangedSkill = false;
    
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

        _rangedSkill = Random.Range(0, 2) == 0;
        
        if (_rangedSkill)
            _skillRange = 10.0f;
        else
            _skillRange = 1.0f;
        
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

            Vector3Int dir = destPos - CellPos;
            if (dir.magnitude <= _skillRange && (dir.x == 0 || dir.y == 0))
            {
                Dir = GetDirFromVector(dir);
                State = CreatureState.Skill;
                
                if(_rangedSkill)
                    _coSkill = StartCoroutine("CoStartShootArrow");
                else
                    _coSkill = StartCoroutine("CoStartPunch");
                return;
            }
        }

        List<Vector3Int> path = Managers.Map.FindPath(CellPos, destPos,  true);
        // 길을 못찾은 경우, 타겟이 있는 경유, 경로가 너무 먼 경우
        if (path.Count < 2 || (_target != null && path.Count > 20)) 
        {
            _target = null;
            State = CreatureState.Idle;
            return;
        }
        
        // 길찾기
        // 0 은 현재 자기가 있는 위치
        Vector3Int nextPos = path[1];
        Vector3Int moveCellDir = nextPos - CellPos; 
        
        Dir = GetDirFromVector(moveCellDir);

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
        Managers.Obj.Remove(Id);
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
    
    IEnumerator CoStartPunch()
    {
        // 피격판정
        GameObject go = Managers.Obj.Find(GetFrontCellPos());
        if (go != null)
        {
            CreatureController cc = go.GetComponent<CreatureController>();
            if(cc != null)
                cc.OnDamaged();
        }

        // 대기
        yield return new WaitForSeconds(0.5f);
        State = CreatureState.Idle;
        _coSkill = null;
    }

    IEnumerator CoStartShootArrow()
    {
        GameObject go = Managers.Resource.Instantiate("Creature/Arrow");
        ArrowController ac = go.GetComponent<ArrowController>();
        ac.Dir = _lastDir;
        ac.CellPos = CellPos;
        
        // 대기
        yield return new WaitForSeconds(0.3f);
        State = CreatureState.Idle;
        _coSkill = null;
    }
}

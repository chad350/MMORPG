using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;
using static Define;

public class MonsterController : CreatureController
{
    private Coroutine _coSkill;

    private bool _rangedSkill = false;
    
    protected override void Init()
    {
        base.Init();

        State = CreatureState.Idle;
        Dir = MoveDir.Down;

        _rangedSkill = Random.Range(0, 2) == 0;

    }

    protected override void UpdateIdle()
    {
        base.UpdateIdle();
    }

    public override void OnDamaged()
    {
        // 몬스터 삭제
        // Managers.Obj.Remove(Id);
        // Managers.Resource.Destroy(gameObject);
    }

    IEnumerator CoStartPunch()
    {
        // 피격판정
        GameObject go = Managers.Obj.FindCreature(GetFrontCellPos());
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
        ac.Dir = Dir;
        ac.CellPos = CellPos;
        
        // 대기
        yield return new WaitForSeconds(0.3f);
        State = CreatureState.Idle;
        _coSkill = null;
    }
}

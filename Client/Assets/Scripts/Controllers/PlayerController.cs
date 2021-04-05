using System;
using System.Collections;
using Google.Protobuf.Protocol;
using UnityEngine;
using static Define;

public class PlayerController : CreatureController
{
    protected Coroutine _coSkill;
    protected bool _rangeSkill = false;
    
    protected override void Init()
    {
        base.Init();
    }

    
    protected override void UpdateAnimation()
    {
        if (State == CreatureState.Idle)
        {
            switch (_lastDir)
            {
                case MoveDir.Up:
                    _animator.Play("idle_back");
                    _sprite.flipX = false;
                    break;
                    
                case MoveDir.Down:
                    _animator.Play("idle_front");
                    _sprite.flipX = false;
                    break;
                    
                case MoveDir.Left:
                    _animator.Play("idle_right");
                    _sprite.flipX = true;
                    break;
                    
                case MoveDir.Right:
                    _animator.Play("idle_right");
                    _sprite.flipX = false;
                    break;
            }
            
        }
        else if (State == CreatureState.Moving)
        {
            switch (Dir)
            {
                case MoveDir.Up:
                    _animator.Play("walk_back");
                    _sprite.flipX = false;
                    break;
                
                case MoveDir.Down:
                    _animator.Play("walk_front");
                    _sprite.flipX = false;
                    break;
                
                case MoveDir.Left:
                    _animator.Play("walk_right");
                    _sprite.flipX = true;
                    break;
                
                case MoveDir.Right:
                    _animator.Play("walk_right");
                    _sprite.flipX = false;
                    break;
            }
        }
        else if (State == CreatureState.Skill)
        {
            switch (_lastDir)
            {
                case MoveDir.Up:
                    _animator.Play(_rangeSkill ? "attack_weapon_back" : "attack_back");
                    _sprite.flipX = false;
                    break;
                
                case MoveDir.Down:
                    _animator.Play(_rangeSkill ? "attack_weapon_front" : "attack_front");
                    _sprite.flipX = false;
                    break;
                
                case MoveDir.Left:
                    _animator.Play(_rangeSkill ? "attack_weapon_right" : "attack_right");
                    _sprite.flipX = true;
                    break;
                
                case MoveDir.Right:
                    _animator.Play(_rangeSkill ? "attack_weapon_right" : "attack_right");
                    _sprite.flipX = false;
                    break;
            }
        }
        else
        {
            
        }
    }
    
    protected override void UpdateController()
    {
        base.UpdateController();
    }

    protected override void UpdateIdle()
    {
        // 이동 상태로 갈지 확인
        if (Dir != MoveDir.None)
        {
            State = CreatureState.Moving;
            return;
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
        _rangeSkill = false;
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
        _rangeSkill = true;
        yield return new WaitForSeconds(0.3f);
        State = CreatureState.Idle;
        _coSkill = null;
    }

    public override void OnDamaged()
    {
        Debug.Log("Player Hit !!");
    }
}

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
        AddHpBar();
    }

    
    protected override void UpdateAnimation()
    {
        if(_animator == null || _sprite == null)
            return;
        
        if (State == CreatureState.Idle)
        {
            switch (Dir)
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
            switch (Dir)
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

    public void UseSkill(int skillId)
    {
        if (skillId == 1)
        {
            _coSkill = StartCoroutine("CoStartPunch");
        }
        else if (skillId == 2)
        {
            _coSkill = StartCoroutine("CoStartShootArrow");
        }
    }

    protected virtual void CheckUpdatedFlag()
    {
    }

    IEnumerator CoStartPunch()
    {
        // 대기
        _rangeSkill = false;
        State = CreatureState.Skill;
        yield return new WaitForSeconds(0.5f);
        State = CreatureState.Idle;
        _coSkill = null;
        CheckUpdatedFlag(); // 임시로직 - Idle 로 바뀐 상태를 서버에 갱신
    }

    IEnumerator CoStartShootArrow()
    {
        // 대기
        _rangeSkill = true;
        State = CreatureState.Skill;
        yield return new WaitForSeconds(0.3f);
        State = CreatureState.Idle;
        _coSkill = null;
        CheckUpdatedFlag(); // 임시로직 - 나중엔 서버에서 처리할 부분
    }

    public override void OnDamaged()
    {
        Debug.Log("Player Hit !!");
    }
}

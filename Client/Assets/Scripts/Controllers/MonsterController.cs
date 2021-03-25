using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class MonsterController : CreatureController
{
    protected override void Init()
    {
        base.Init();
        State = CreatureState.Idle;
        Dir = MoveDir.None;
    }

    protected override void UpdateController()
    {
        //GetDirInput();
        base.UpdateController();
    }

 
    void GetDirInput()
    {
        if (Input.GetKey(KeyCode.W))
        {
            Dir = MoveDir.Up;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            Dir = MoveDir.Down;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            Dir = MoveDir.Left;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            Dir = MoveDir.Right;
        }
        else
        {
            Dir = MoveDir.None;
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
}

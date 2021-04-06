﻿using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;
using static Define;

public class ArrowController : CreatureController
{
    protected override void Init()
    {
        switch (Dir)
        {
            case MoveDir.Up:
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case MoveDir.Down:
                transform.rotation = Quaternion.Euler(0, 0, -180);
                break;
            case MoveDir.Left:
                transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case MoveDir.Right:
                transform.rotation = Quaternion.Euler(0, 0, -90);
                break;
        }

        State = CreatureState.Moving;
        _speed = 15.0f;
        
        base.Init();
    }

    protected override void UpdateAnimation()
    {
        
    }

    protected override void MoveToNextPosition()
    {
        Vector3Int destPos = CellPos; 
        switch (Dir)
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

        State = CreatureState.Moving;
        if (Managers.Map.CanGo(destPos))
        {
            GameObject go = Managers.Obj.Find(destPos);
            if (go == null)
            {
                CellPos = destPos;
            }
            else
            {
                // 상대 판정
                CreatureController cc = go.GetComponent<CreatureController>();
                if(cc != null)
                    cc.OnDamaged();

                // 화살 삭제
                Managers.Resource.Destroy(gameObject);
            }
        }
        else
        {
            Managers.Resource.Destroy(gameObject);
        }
    }
}

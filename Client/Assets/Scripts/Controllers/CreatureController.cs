﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using Google.Protobuf.Protocol;
using UnityEngine;

public class CreatureController : MonoBehaviour
{
    public int Id { get; set; }

    public float _speed = 5.0f;

    // 더티 플래그 - dirty flag
    protected bool _updated = false;
    
    PositionInfo _positionInfo = new PositionInfo();
    public PositionInfo PosInfo
    {
        get { return _positionInfo;}
        set 
        { 
            if(_positionInfo.Equals(value))
                return;
            
            CellPos = new Vector3Int(value.PoxX, value.PoxY, 0);
            State = value.State;
            Dir = value.MoveDir;
        }
    }

    public void SyncPos()
    {
        Vector3 dest = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.5f, 0.5f);
        transform.position = dest;
    }

    public Vector3Int CellPos 
    {
        get
        {
            return new Vector3Int(PosInfo.PoxX, PosInfo.PoxY, 0);
        }
        set
        {
            if(PosInfo.PoxX == value.x && PosInfo.PoxY == value.y)
                return;
            
            PosInfo.PoxX = value.x;
            PosInfo.PoxY = value.y;
            _updated = true;
        }
    }
    protected Animator _animator;
    protected SpriteRenderer _sprite;
    
    public virtual CreatureState State
    {
        get
        {
            return PosInfo.State;
        }
        set
        {
            if(PosInfo.State == value)
                return;
            
            PosInfo.State = value;
            UpdateAnimation();
            _updated = true;
        }
    }

    [SerializeField]
    protected MoveDir _lastDir = MoveDir.Down;

    public MoveDir Dir
    {
        get { return PosInfo.MoveDir;}
        set
        {
            if(PosInfo.MoveDir == value)
                return;
            
            PosInfo.MoveDir = value;
            if (value != MoveDir.None)
                _lastDir = value;

            UpdateAnimation();
            _updated = true;
        }
    }

    public MoveDir GetDirFromVector(Vector3Int dir)
    {
        if (dir.x > 0)
            return MoveDir.Right;
        else if (dir.x < 0)
            return MoveDir.Left;
        else if (dir.y > 0)
            return MoveDir.Up;
        else if (dir.y < 0)
            return MoveDir.Down;
        else
            return MoveDir.None;
    }

    public Vector3Int GetFrontCellPos()
    {
        Vector3Int cellPos = CellPos;
        switch (_lastDir)
        {
            case MoveDir.Up:
                cellPos += Vector3Int.up;
                break;
            case MoveDir.Down:
                cellPos += Vector3Int.down;
                break;
            case MoveDir.Left:
                cellPos += Vector3Int.left;
                break;
            case MoveDir.Right:
                cellPos += Vector3Int.right;
                break;
        }

        return cellPos;
    }

    protected virtual void UpdateAnimation()
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
                    _animator.Play("attack_back");
                    _sprite.flipX = false;
                    break;
                
                case MoveDir.Down:
                    _animator.Play("attack_front");
                    _sprite.flipX = false;
                    break;
                
                case MoveDir.Left:
                    _animator.Play("attack_right");
                    _sprite.flipX = true;
                    break;
                
                case MoveDir.Right:
                    _animator.Play("attack_right");
                    _sprite.flipX = false;
                    break;
            }
        }
        else
        {
            
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateController();
    }

    protected virtual void Init()
    {
        _animator = GetComponent<Animator>();
        _sprite = GetComponent<SpriteRenderer>();
        Vector3 pos = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.5f, 0.5f);
        transform.position = pos;
        
        State = CreatureState.Idle;
        Dir = MoveDir.None;
        UpdateAnimation();
    }

    protected virtual void UpdateController()
    {
        switch (State)
        {
            case CreatureState.Idle:
                UpdateIdle();
                break;
            case CreatureState.Moving:
                UpdateMoving();
                break;
            case CreatureState.Skill:
                break;
            case CreatureState.Dead:
                break;
        }
    }


    protected virtual void UpdateIdle()
    {

    }
    
    protected virtual void UpdateMoving()
    {
        Vector3 dest = Managers.Map.CurrentGrid.CellToWorld(CellPos) + new Vector3(0.5f, 0.5f) ;
        Vector3 moveDir = dest - transform.position;
        
        // 방향 벡터는 2가지의 크기를가지고 있다.
        // 실제 이동하는 방향
        // 이동하려는 목적지까지의 크기
        
        // 도착 여부
        // 방향벡터의 크기
        // 목적지까지 얼마나남았는지 추출
        float dist = moveDir.magnitude;
        if (dist < _speed * Time.deltaTime)
        {
            transform.position = dest;
            MoveToNextPosition();
        }
        else
        {
            transform.position += moveDir.normalized * _speed * Time.deltaTime;
            State = CreatureState.Moving;
        }
        
    }

    protected virtual void MoveToNextPosition()
    {
        
    }

    protected virtual void UpdateSkill()
    {
        
    }

    protected virtual void UpdateDead()
    {
        
    }

    public virtual void OnDamaged()
    {
        
    }
}

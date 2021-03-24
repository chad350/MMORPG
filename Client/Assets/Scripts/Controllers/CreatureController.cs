using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static Define;
public class CreatureController : MonoBehaviour
{
    public float _speed = 5.0f;

    public Vector3Int CellPos { get; set; } = Vector3Int.zero;
    protected Animator _animator;
    protected SpriteRenderer _sprite;
    
    private CreatureState _state = CreatureState.Idle;

    public CreatureState State
    {
        get => _state;
        set
        {
            if(_state == value)
                return;
            
            _state = value;
            UpdateAnimation();
        }
    }

    private MoveDir _lastDir = MoveDir.Down;
    private MoveDir _dir = MoveDir.Down;
    public MoveDir Dir
    {
        get { return _dir;}
        set
        {
            if(_dir == value)
                return;
            
            _dir = value;
            if (value != MoveDir.None)
                _lastDir = value;

            UpdateAnimation();
        }
    }

    protected virtual void UpdateAnimation()
    {
        if (_state == CreatureState.Idle)
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
        else if (_state == CreatureState.Moving)
        {
            switch (_dir)
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
        else if (_state == CreatureState.Skill)
        {
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
    }

    protected virtual void UpdateController()
    {
        UpdatePosition();
        UpdateIsMoving();
    }


    void UpdateIsMoving()
    {
        if (State == CreatureState.Idle && _dir != MoveDir.None)
        {
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

            State = CreatureState.Moving;
            if (Managers.Map.CanGo(destPos))
            {
                if (Managers.Obj.Find(destPos) == null)
                {
                    CellPos = destPos;
                }
            }
        }
    }
    
    void UpdatePosition()
    {
        if(State != CreatureState.Moving)
            return;
        
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
            // 예외적으로 애니메이션을 직접 컨트롤
            _state = CreatureState.Idle;
            if(_dir == MoveDir.None)
                UpdateAnimation();
        }
        else
        {
            transform.position += moveDir.normalized * _speed * Time.deltaTime;
            State = CreatureState.Moving;
        }
        
    }
}

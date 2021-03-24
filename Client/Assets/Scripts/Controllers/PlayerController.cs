using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class PlayerController : MonoBehaviour
{
    public Grid _grid;
    private float _speed = 5.0f;

    private Vector3Int _cellPos = Vector3Int.zero;
    private bool _isMoving = false;

    private Animator _animator;
    
    private MoveDir _dir = MoveDir.Down;
    public MoveDir Dir
    {
        get { return _dir;}
        set
        {
            if(_dir == value)
                return;

            switch (value)
            {
                case MoveDir.Up:
                    _animator.Play("walk_back");
                    transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    break;
                
                case MoveDir.Down:
                    _animator.Play("walk_front");
                    transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    break;
                
                case MoveDir.Left:
                    _animator.Play("walk_right");
                    transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
                    break;
                
                case MoveDir.Right:
                    _animator.Play("walk_right");
                    transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    break;
                
                case MoveDir.None:
                    if (_dir == MoveDir.Up)
                    {
                        _animator.Play("idle_back");
                        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    }
                    else if (_dir == MoveDir.Down)
                    {
                        _animator.Play("idle_front");
                        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    }
                    else if (_dir == MoveDir.Left)
                    {
                        _animator.Play("idle_left");
                        transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
                    }
                    else
                    {
                        _animator.Play("idle_right");
                        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    }
                    break;
            }

            _dir = value;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        Vector3 pos = _grid.CellToWorld(_cellPos) + new Vector3(0.5f, 0.5f);
        transform.position = pos;
    }

    // Update is called once per frame
    void Update()
    {
        GetDirInput();
        UpdatePosition();
        UpdateIsMoving();
        
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

    void UpdateIsMoving()
    {
        if (_isMoving == false)
        {
            switch (_dir)
            {
                case MoveDir.Up:
                    _cellPos += Vector3Int.up;
                    _isMoving = true;
                    break;
            
                case MoveDir.Down:
                    _cellPos += Vector3Int.down;
                    _isMoving = true;
                    break;
            
                case MoveDir.Left:
                    _cellPos += Vector3Int.left;
                    _isMoving = true;
                    break;
            
                case MoveDir.Right:
                    _cellPos += Vector3Int.right;
                    _isMoving = true;
                    break;
            }
        }
    }
    
    void UpdatePosition()
    {
        if(_isMoving == false)
            return;
        
        Vector3 dest = _grid.CellToWorld(_cellPos) + new Vector3(0.5f, 0.5f) ;
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
            _isMoving = false;
        }
        else
        {
            transform.position += moveDir.normalized * _speed * Time.deltaTime;
            _isMoving = true;
        }
        
    }
}

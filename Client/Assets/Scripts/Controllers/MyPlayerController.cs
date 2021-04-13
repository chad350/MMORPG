using System.Collections;
using Google.Protobuf.Protocol;
using UnityEngine;
using static Define;

public class MyPlayerController : PlayerController
{
    private bool _moveKeyPressed = false;
    
    protected override void Init()
    {
        base.Init();
    }
        
    private void LateUpdate()
    {
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }
    
    protected override void UpdateController()
    {
        GetUIKeyInput();
        switch (State)
        {
            case CreatureState.Idle:
                GetDirInput();
                UpdateIdle();
                break;
            case CreatureState.Moving:
                GetDirInput();
                break;
        }
        GetDirInput();
        base.UpdateController();
    }

    void GetUIKeyInput()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            // UI 에서 표시
            UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
            UI_Inventory invenUI = gameSceneUI.InvenUI;

            if (invenUI.gameObject.activeSelf)
            {
                invenUI.gameObject.SetActive(false);
            }
            else
            {
                invenUI.gameObject.SetActive(true);
                invenUI.RefreshUI();
            }
        }
    }

    void GetDirInput()
    {
        _moveKeyPressed = true;
        
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
            _moveKeyPressed = false;
        }    
    }
    
    protected override void UpdateIdle()
    {
        // 이동 상태로 갈지 확인
        if (_moveKeyPressed)
        {
            State = CreatureState.Moving;
            return;
        }

        // 스킬 상태로 갈지 확인
        if (_coSkillCooltime == null && Input.GetKey(KeyCode.Space))
        {
            Debug.Log("Skill !");
            
            C_Skill skill = new C_Skill() {Info = new SkillInfo() };
            skill.Info.SkillId = 2;
            Managers.Network.Send(skill);

            _coSkillCooltime = StartCoroutine("CoInputCooltiime", 0.2f);
        }
    }

    private Coroutine _coSkillCooltime;
    IEnumerator CoInputCooltiime(float time)
    {
        yield return new WaitForSeconds(time);
        _coSkillCooltime = null;
    }

    protected override void MoveToNextPosition()
    {
        if (_moveKeyPressed == false)
        {
            State = CreatureState.Idle;
            CheckUpdatedFlag();
            return;
        }
        
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
        
        if (Managers.Map.CanGo(destPos))
        {
            if (Managers.Obj.FindCreature(destPos) == null)
            {
                CellPos = destPos;
            }
        }

        CheckUpdatedFlag();
    }

    protected override void CheckUpdatedFlag()
    {
        if (_updated)
        {
            C_Move movePacket = new C_Move();
            movePacket.PosInfo = PosInfo;
            Managers.Network.Send(movePacket);
            _updated = false;
        }
    }
}

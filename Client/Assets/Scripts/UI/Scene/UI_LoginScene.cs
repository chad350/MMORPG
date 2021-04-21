using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_LoginScene : UI_Scene
{
    enum GameObjects
    {
        AccountName,
        Password
    }

    enum Images
    {
        CreateBtn,
        LoginBtn
    }
    
    public override void Init()
    {
        base.Init();
        
        Bind<GameObject>(typeof(GameObjects));
        Bind<Image>(typeof(Images));
        
        GetImage((int)Images.CreateBtn).gameObject.BindEvent(OnClickCreateButton);
        GetImage((int)Images.LoginBtn).gameObject.BindEvent(OnClickLoginButton);
    }

    public void OnClickCreateButton(PointerEventData evt)
    {
        
        string account = Get<GameObject>((int) GameObjects.AccountName).GetComponent<InputField>().text;
        string password = Get<GameObject>((int) GameObjects.Password).GetComponent<InputField>().text;

        CreateAccountPacketReq packet = new CreateAccountPacketReq()
        {
            AccountName = account,
            Password = password
        };
        
        Managers.Web.SendPostRequest<CreateAccountPacketRecv>("account/create", packet, (recv) =>
        {
            Debug.Log("CreateAccountPacketRecv : " + recv.CreateOk);
        });
    }
    
    public void OnClickLoginButton(PointerEventData evt)
    {
        string account = Get<GameObject>((int) GameObjects.AccountName).GetComponent<InputField>().text;
        string password = Get<GameObject>((int) GameObjects.Password).GetComponent<InputField>().text;
        
        LoginAccountPacketReq packet = new LoginAccountPacketReq()
        {
            AccountName = account,
            Password = password
        };
        
        Managers.Web.SendPostRequest<LoginAccountPacketRecv>("account/login", packet, (recv) =>
        {
            Debug.Log("LoginAccountPacketRecv : " + recv.LoginOk);

            if (recv.LoginOk)
            {
                Managers.Network.AccountId = recv.AccountId;
                Managers.Network.Token = recv.Token;
                
                UI_SelectServerPopup popup = Managers.UI.ShowPopupUI<UI_SelectServerPopup>();
                popup.SetServer(recv.ServerList);
            }
        });
    }
}
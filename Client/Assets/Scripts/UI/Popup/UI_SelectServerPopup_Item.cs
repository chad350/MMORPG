using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_SelectServerPopup_Item : UI_Base
{
    public ServerInfo Info { get; set; }

    enum Buttons
    {
        btnSelectServer
    }

    enum Texts
    {
        txtName
    }
    
    public override void Init()
    {
        Bind<Button>(typeof(Buttons));
        Bind<Text>(typeof(Texts));
                    
        GetButton((int)Buttons.btnSelectServer).gameObject.BindEvent(OnClickButton);
    }

    public void RefreshUI()
    {
        if(Info == null)
            return;
        
        GetText((int) Texts.txtName).text = Info.Name;
    }

    void OnClickButton(PointerEventData evt)
    {
        Managers.Network.ConnectToGame(Info);
        Managers.Scene.LoadScene(Define.Scene.Game);
        Managers.UI.ClosePopupUI();
    }
}

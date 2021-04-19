﻿using System.Collections.Generic;
using UnityEngine;

public class CreateAccountPacketReq
{
    public string AccountName;
    public string Password;
}

public class CreateAccountPacketRecv
{
    public bool CreateOk;
}

public class LoginAccountPacketReq
{
    public string AccountName;
    public string Password;
}

public class ServerInfo
{
    public string Name;
    public string Ip;
    public int CrowdedLevel;
}

public class LoginAccountPacketRecv
{
    public bool LoginOk;
    public List<ServerInfo> ServerList = new List<ServerInfo>();
}

public class WebPacket
{
    public static void SendCreateAccount(string account, string password)
    {
        CreateAccountPacketReq packet = new CreateAccountPacketReq()
        {
            AccountName = account,
            Password = password
        };
        
        Managers.Web.SendPostRequest<CreateAccountPacketRecv>("account/create", packet, (recv) =>
        {
            Debug.Log(recv.CreateOk);
        });
    }
    
    public static void SendLoginAccount(string account, string password)
    {
        LoginAccountPacketReq packet = new LoginAccountPacketReq()
        {
            AccountName = account,
            Password = password
        };
        
        Managers.Web.SendPostRequest<LoginAccountPacketRecv>("account/login", packet, (recv) =>
        {
            Debug.Log(recv.LoginOk);
        });
    }
}
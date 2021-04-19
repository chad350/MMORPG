using System.Collections.Generic;
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
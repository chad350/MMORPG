using System;
using System.Diagnostics;
using System.Net.Mime;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;

class PacketHandler
{
	// step 4
	public static void S_EnterGameHandler(PacketSession session, IMessage packet)
	{
		S_EnterGame enterGamePacket = packet as S_EnterGame;
		ServerSession serverSession = (ServerSession) session;
		
		
	}
	
	public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
	{
		S_LeaveGame leaveGamePacket = packet as S_LeaveGame;
	}
	
	public static void S_SpawnHandler(PacketSession session, IMessage packet)
	{
		S_Spawn spawnPacket = packet as S_Spawn;
	}
	
	public static void S_DespawnHandler(PacketSession session, IMessage packet)
	{
		S_Despawn despawnPacket = packet as S_Despawn;
	}
	
	public static void S_MoveHandler(PacketSession session, IMessage packet)
	{
		S_Move movePacket = packet as S_Move;
	}

	public static void S_SkillHandler(PacketSession session, IMessage packet)
	{
		S_Skill skillPacket = packet as S_Skill;
	}
	
	public static void S_ChangeHpHandler(PacketSession session, IMessage packet)
	{
		S_ChangeHp changePacket = packet as S_ChangeHp;
	}
	
	public static void S_DieHandler(PacketSession session, IMessage packet)
	{
		S_Die diePacket = packet as S_Die;
	}
	
	// step 1
	public static void S_ConnectedHandler(PacketSession session, IMessage packet)
	{
		C_Login loginPacket = new  C_Login();
		ServerSession serverSession = (ServerSession) session;
		
		loginPacket.UniqueId = $"DummyClient_{serverSession.DummyId.ToString("0000")}";
		serverSession.Send(loginPacket);
	}
	
	// step 2
	// 로그인 OK + 캐릭터 목록
	public static void S_LoginHandler(PacketSession session, IMessage packet)
	{
		S_Login LoginPacket = packet as S_Login;
		ServerSession serverSession = (ServerSession) session;
		
		// 로비 UI - 캐릭터 보여주고 선택

		if (LoginPacket.Players == null || LoginPacket.Players.Count == 0)
		{
			C_CreatePlayer createPacket = new C_CreatePlayer();
			createPacket.Name = $"Player_{serverSession.DummyId.ToString("0000")}";
			serverSession.Send(createPacket);
		}
		else
		{
			// 첫번째 캐릭터 로그인
			LobbyPlayerInfo info = LoginPacket.Players[0];
			C_EnterGame enterGamePacket = new C_EnterGame();
			enterGamePacket.Name = info.Name;
			serverSession.Send(enterGamePacket);
		}
	}
	
	// step 3
	public static void S_CreatePlayerHandler(PacketSession session, IMessage packet)
	{
		S_CreatePlayer createOkPacket = packet as S_CreatePlayer;
		ServerSession serverSession = (ServerSession) session;
		
		if (createOkPacket.Player == null)
		{
			C_CreatePlayer createPacket = new C_CreatePlayer();
			createPacket.Name = $"Player_{serverSession.DummyId.ToString("0000")}";
			serverSession.Send(createPacket);
		}
		else
		{
			C_EnterGame enterGamePacket = new C_EnterGame();
			enterGamePacket.Name = createOkPacket.Player.Name;
			serverSession.Send(enterGamePacket);
		}
	}
	
	public static void S_ItemListHandler(PacketSession session, IMessage packet)
	{
		S_ItemList itemListPacket = packet as S_ItemList;
	}
	
	public static void S_AddItemHandler(PacketSession session, IMessage packet)
	{
		S_AddItem itemListPacket = packet as S_AddItem;
	}
	
	public static void S_EquipItemHandler(PacketSession session, IMessage packet)
	{
		S_EquipItem equipItemPacket = packet as S_EquipItem;
	}
	
	public static void S_ChangeStatHandler(PacketSession session, IMessage packet)
	{
		S_ChangeStat changeStatePacket = packet as S_ChangeStat;
	}
	
	public static void S_PingHandler(PacketSession session, IMessage packet)
	{
		C_Pong pingPacket = new C_Pong();
	}
}
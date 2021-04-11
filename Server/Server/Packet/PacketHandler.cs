﻿using Server;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.DB;
using Server.Game;

class PacketHandler
{
	public static void C_MoveHandler(PacketSession session, IMessage packet)
	{
		C_Move movePacket = packet as C_Move;
		ClientSession clientSession = session as ClientSession;
		
		// 사용하기전에 한번 캐싱해서 사용
		Player player = clientSession.MyPlayer;
		if(player == null)
			return;

		GameRoom room = player.Room;
		if(room == null)
			return;

		room.JobQ.Push(room.HandleMove, player, movePacket);
	}
	
	public static void C_SkillHandler(PacketSession session, IMessage packet)
	{
		C_Skill skillPacket = packet as C_Skill;
		ClientSession clientSession = session as ClientSession;
		
		Player player = clientSession.MyPlayer;
		if(player == null)
			return;

		GameRoom room = player.Room;
		if(room == null)
			return;

		room.JobQ.Push(room.HandleSkill, player, skillPacket);
	}
	
	public static void C_LoginHandler(PacketSession session, IMessage packet)
	{
		C_Login loginPacket = packet as C_Login;
		ClientSession clientSession = session as ClientSession;
		clientSession.HandleLogin(loginPacket);
	}
	
	public static void C_EnterGameHandler(PacketSession session, IMessage packet)
	{
		C_EnterGame enterGamePacket = packet as C_EnterGame;
		ClientSession clientSession = session as ClientSession;

		clientSession.HandleEnterGame(enterGamePacket);
	}
	
	public static void C_CreatePlayerHandler(PacketSession session, IMessage packet)
	{
		C_CreatePlayer createPlayerPacket = packet as C_CreatePlayer;
		ClientSession clientSession = session as ClientSession;

		clientSession.HandleCreatePlayer(createPlayerPacket);
	}
}

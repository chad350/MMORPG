using Server;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Game;

class PacketHandler
{
	public static void C_MoveHandler(PacketSession session, IMessage packet)
	{
		C_Move movePacket = packet as C_Move;
		ClientSession clientSession = session as ClientSession;

		Console.WriteLine($"C_Move - {movePacket.PosInfo.PoxX}, {movePacket.PosInfo.PoxY}");
		
		// 사용하기전에 한번 캐싱해서 사용
		Player player = clientSession.MyPlayer;
		if(player == null)
			return;

		GameRoom room = player.Room;
		if(room == null)
			return;

		room.HandleMove(player, movePacket);
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

		room.HandleSkill(player, skillPacket);
	}
}

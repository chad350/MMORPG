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
		
		// 검증 단계 - 클라는 거짓말을 하기 때문
		
		// 서버에서 좌표 이동
		PlayerInfo info = player.info;
		info.PosInfo = movePacket.PosInfo;
		
		// 다른 플레이어에게 브로드캐스트
		S_Move resMovePacket = new S_Move();
		resMovePacket.PlayerId = player.info.PlayerId;
		resMovePacket.PosInfo = movePacket.PosInfo;
		
		room.Broadcast(resMovePacket);
	}
}

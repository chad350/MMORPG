using Server;
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

		Console.WriteLine($"UniqueId : {loginPacket.UniqueId}");
		
		// 보완 체크
		
		// 문제가 있긴 하다.
		using (AppDbContext db = new AppDbContext())
		{
			AccountDb findAccount = db.Accounts
				.Where(a => a.AccountName == loginPacket.UniqueId)
				.FirstOrDefault();
			
			if(findAccount != null)
			{
				// 찾은 계정이 있다면 로그인 
				S_Login loginOk = new S_Login() { LoginOk = 1 };
				clientSession.Send(loginOk);
			}
			else
			{
				// 찾은 계정이 없다면 계정 생성 후 로그인 
				AccountDb newAccount = new AccountDb() {AccountName = loginPacket.UniqueId};
				db.Accounts.Add(newAccount);
				db.SaveChanges();
				
				S_Login loginOk = new S_Login() { LoginOk = 1 };
				clientSession.Send(loginOk);
			}
		}
	}
}

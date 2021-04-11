using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using Server.Game;
using ServerCore;
using Timer = System.Timers.Timer;

namespace Server
{
	class Program
	{
		static Listener _listener = new Listener();
		private static List<Timer> _timers = new List<Timer>();
		
		static void TickRoom(GameRoom room, int tick = 100)
		{
			var timer = new Timer();
			timer.Interval = tick;
			timer.Elapsed += ((s, e) => { room.Update(); });
			timer.AutoReset = true;
			timer.Enabled = true;
			
			_timers.Add(timer);
		}

		static void Main(string[] args)
		{
			ConfigManager.LoadConfig();
			DataManager.LoadData();

			// DB 테스트
			using (AppDbContext db = new AppDbContext())
			{
				db.Accounts.Add(new AccountDb() {AccountName = "TestAccount"});
				// AccountName 는 유니크 인덱스로 설정해뒀기 때문에 최초 실행 이후에는 에러가 발생
				db.SaveChanges();
			}
			
			// 3. Timer 를 이용해서 필요할 때에 호출한다.
			// 기존 방식과 다르게 메인쓰레드가 아닌 다른 쓰레드에서 분산해서 일감을 처리
			GameRoom room = RoomManager.Instance.Add(1);
			TickRoom(room, 50);
			
			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			_listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
			Console.WriteLine("Listening...");
			
			while (true)
			{
				// Room Update
				// 업데이트에 관한건 장르에 따라 각자 다르다
				// LOL, FPS - 비교적 자주 체크 해야한다 (0.1초에 한번은 해야하지 않을까?)
				// MMO - 위에 비해 자주 안해도 되지만 적당한 주기를 찾는것이 중요

				// 1. 무시하지만 단순한 방법  - 일종의 폴링 방식
				//RoomManager.Instance.Find(1).Update();
				//Thread.Sleep(100);

				// 2. 폴링 방식을 유지하지만 요청을 직접 실행하는게 아니라 JobQ로 핸들링
				// 1번에 비하면 lock 쏠림을 피할수 있지만 여전히 폴링 방식의 문제를 가지고 있다.
				// GameRoom room = RoomManager.Instance.Find(1);
				// room.JobQ.Push(room.Update);
				// Thread.Sleep(100);
			}
		}
	}
}

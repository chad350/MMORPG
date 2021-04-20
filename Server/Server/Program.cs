using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using Server.Game;
using Server.Utils;
using ServerCore;
using SharedDB;
using Timer = System.Timers.Timer;

namespace Server
{
	// 게임 서버 작업 과정
	// 1. GameRoom 방식의 간단한 동기화 <- OK
	// 2. 더 넓은 영역 관리
	// 3. 심리스 MMO
	
	// GameLogic 에서 통신할 때
	// 1. Recv (N개) 
	// 2. GameLogic (단일 쓰레드) - 네트워크 통신은 컨텍스트 스위칭으로 부하가 걸린다. 게임 로직에 영향을 줄 수 있다.
	// 3. DB (단일 쓰레드 - 메인)
	// - >
	// 네트워크 로직을 분리해서 처리
	// 1. Recv (N개) 
	// 2. GameLogic (단일 쓰레드)
	// 3. Send (단일 쓰레드) - 통신을 게임 로직과 분리해 게임 로직은 좀 더 원활하게 처리 된다.
	// 4. DB (단일 쓰레드 - 메인)
	
	class Program
	{
		// 타이머 방식에서 필요했던 함수
		// private static List<Timer> _timers = new List<Timer>();
		//
		// static void TickRoom(GameRoom room, int tick = 100)
		// {
		// 	var timer = new Timer();
		// 	timer.Interval = tick;
		// 	timer.Elapsed += ((s, e) => { room.Update(); });
		// 	timer.AutoReset = true;
		// 	timer.Enabled = true;
		// 	
		// 	_timers.Add(timer);
		// }

		static Listener _listener = new Listener();

		static void GameLogicTask()
		{
			while (true)
			{
				GameLogic.Instance.Update();
				Thread.Sleep(0);
			}
		}

		static void DbTask()
		{
			while (true)
			{
				DbTransaction.Instance.Flush();
				// 자신의 소유권 양도 - cpu 과도 사용 대비
				// Flush 에서 어짜피 일감이 없어질 때 까지 처리
				Thread.Sleep(0);
			}
		}

		// 나중에 요청이 많아지면 Task 를 여러개 둘 수도 있다.
		static void NetworkTask()
		{
			// 매번 새 리스트를 만들어서 처리하는게 부하일 수 있지만
			// 패킷을 보내는 부하에 비하면 가벼운 처리
			// 게임 로직에서 처리를 분산하려면 감당해야한다.
			while (true)
			{
				List<ClientSession> sessions = SessionManager.Instance.GetSessions();
				foreach (var session in sessions)
					session.FlushSend();
				
				Thread.Sleep(0);
			}
		}

		static void StartServerInfoTask()
		{
			var t = new System.Timers.Timer();
			t.AutoReset = true;
			t.Elapsed += new ElapsedEventHandler((s, e) =>
			{
				using (SharedDbContext shared = new SharedDbContext())
				{
					ServerDb serverDb = shared.Servers.Where(s => s.Name == Name).FirstOrDefault();
					if (serverDb != null)
					{
						serverDb.IpAddress = IpAddress;
						serverDb.Port = Port;
						serverDb.BusyScore = SessionManager.Instance.GetBusyScore();
						shared.SaveChangesEx();
					}
					else
					{
						serverDb = new ServerDb()
						{
							Name = Program.Name,
							IpAddress = Program.IpAddress,
							Port = Program.Port,
							BusyScore = SessionManager.Instance.GetBusyScore()
						};
						shared.Servers.Add(serverDb);
						shared.SaveChangesEx();
					}
				}
			});
			t.Interval = 10 * 1000;
			t.Start();
		}

		public static string Name { get; } = "302";
		public static int Port { get; } = 7777;
		public static string IpAddress { set; get; }

		static void Main(string[] args)
		{
			ConfigManager.LoadConfig();
			DataManager.LoadData();
			
			GameLogic.Instance.JobQ.Push(() => { GameRoom room = GameLogic.Instance.Add(1); });
			
			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[1];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, Port);

			IpAddress = ipAddr.ToString();
			
			_listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
			Console.WriteLine("Listening...");

			// ServerInfo Task
			StartServerInfoTask();
			
			// DbTask
			{
				// thread 를 만드나 task 를 Long Running 으로 하나 별 차이는 없다
				Thread t_Db = new Thread(DbTask);
				t_Db.Name = "DB";
				t_Db.Start();
			}

			// NetworkTask
			{
				Thread t_Network = new Thread(NetworkTask);
				t_Network.Name = "Network";
				t_Network.Start();
			}
			
			// GameLogicTask
			{
				Thread.CurrentThread.Name = "GameLogic";
				GameLogicTask();
			}


			
			
			// Room Update
			// 업데이트에 관한건 장르에 따라 각자 다르다
			// LOL, FPS - 비교적 자주 체크 해야한다 (0.1초에 한번은 해야하지 않을까?)
			// MMO - 위에 비해 자주 안해도 되지만 적당한 주기를 찾는것이 중요
			
			// 1. Timer 를 이용해서 필요할 때에 호출한다.
			// 기존 방식과 다르게 메인쓰레드가 아닌 다른 쓰레드에서 분산해서 일감을 처리
			//TickRoom(room, 50);

			// while (true)
			// {
				// 2. 무시하지만 단순한 방법  - 일종의 폴링 방식
				//RoomManager.Instance.Find(1).Update();
				//Thread.Sleep(100);

				// 3. 폴링 방식을 유지하지만 요청을 직접 실행하는게 아니라 JobQ로 핸들링
				// 1번에 비하면 lock 쏠림을 피할수 있지만 여전히 폴링 방식의 문제를 가지고 있다.
				// GameRoom room = RoomManager.Instance.Find(1);
				// room.JobQ.Push(room.Update);
				// Thread.Sleep(100);
			// }
		}
	}
}

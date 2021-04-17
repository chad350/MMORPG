using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;
using System.Net;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using Server.Game;

namespace Server
{
	public partial class ClientSession : PacketSession
	{
		public PlayerServerState ServerState { get; private set; } = PlayerServerState.ServerStateLogin;
		
		// 현재 관리하고 있는 플레이어를 알면 코드 작성이 쉬워 진다.
		public Player MyPlayer { get; set; }
		public int SessionId { get; set; }

		private object _lock = new object();
		private List<ArraySegment<byte>> _reserveQueue = new List<ArraySegment<byte>>();

		#region Ping Pong

		private long _pingpongTick = 0;

		public void Ping()
		{
			if (_pingpongTick > 0)
			{
				long delta = (Environment.TickCount64 - _pingpongTick);
				if (delta > 30 * 1000)
				{
					Console.WriteLine("Disconnected by PingCheck");
					Disconnect();
					return;
				}
			}

			S_Ping pingPacket = new S_Ping();
			Send(pingPacket);

			GameLogic.Instance.JobQ.PushAfter(5000, Ping);
		}

		public void HandlePong()
		{
			_pingpongTick = Environment.TickCount64;
		}
		
		#endregion
		
		#region Network
		// 요청을 예약만 한다.
		public void Send(IMessage packet)
		{
			string msgName = packet.Descriptor.Name.Replace("_", String.Empty);
			MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), msgName);
			
			ushort size = (ushort)packet.CalculateSize();
			byte[] sendBuff = new byte[size + 4]; 
			Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuff, 0, sizeof(ushort)); // 어느정도 크기의 데이터인지
			Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuff, 2, sizeof(ushort)); // 프로토콜의 아이디
			Array.Copy(packet.ToByteArray(), 0, sendBuff, 4, size); // 전달하려는 데이터
			
			// Send 는 부하가 상당히 큰 작업
			// 데이터를 전송할 떄 컨텍스트 스위칭이 일어난다.
			// GameLogic Update -> Room Send 로 실행되는 이 부분이 부하가 크면 앞의 로직들이 문제가 생길 수 있다. 
			// Send(new ArraySegment<byte>(sendBuff));
			
			// 직접 호출하지 않고 실행을 위임
			lock (_lock)
			{
				_reserveQueue.Add(sendBuff);	
			}
		}

		// 예약된 요청을 실제로 실행한다.
		public void FlushSend()
		{
			List<ArraySegment<byte>> sendList = null;
			// 락을 걸고 데이터를 보내면 Flush 가 다 처리되기전엔 새로운 요청을 받을 수 없다
			// 락을 걸고 전송할 데이터만 새로 뽑아온 뒤 기존 요청은 초기화
			// 이후 락을 빠져 나온 뒤 Send 를 실행한다.
			lock (_lock)
			{
				if(_reserveQueue.Count == 0)
					return;
				
				sendList = _reserveQueue;
				_reserveQueue = new List<ArraySegment<byte>>();
			}
			
			Send(sendList);
		}

		public override void OnConnected(EndPoint endPoint)
		{
			Console.WriteLine($"OnConnected : {endPoint}");

			{
				S_Connected connectedPacket = new S_Connected();
				Send(connectedPacket);
			}

			GameLogic.Instance.JobQ.PushAfter(5000, Ping);
		}

		public override void OnRecvPacket(ArraySegment<byte> buffer)
		{
			PacketManager.Instance.OnRecvPacket(this, buffer);
		}

		public override void OnDisconnected(EndPoint endPoint)
		{
			GameLogic.Instance.JobQ.Push(() =>
			{
				if(MyPlayer == null)
					return;
				
				GameRoom room = GameLogic.Instance.Find(1);
				room.JobQ.Push(room.LeaveGame, MyPlayer.info.ObjectId); 
			});

			SessionManager.Instance.Remove(this);

			Console.WriteLine($"OnDisconnected : {endPoint}");
		}

		public override void OnSend(int numOfBytes)
		{
			//Console.WriteLine($"Transferred bytes: {numOfBytes}");
		}
		#endregion
	}
}

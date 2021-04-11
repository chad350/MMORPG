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

		#region Network
		public void Send(IMessage packet)
		{
			string msgName = packet.Descriptor.Name.Replace("_", String.Empty);
			MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), msgName);
			
			ushort size = (ushort)packet.CalculateSize();
			byte[] sendBuff = new byte[size + 4]; 
			Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuff, 0, sizeof(ushort)); // 어느정도 크기의 데이터인지
			Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuff, 2, sizeof(ushort)); // 프로토콜의 아이디
			Array.Copy(packet.ToByteArray(), 0, sendBuff, 4, size); // 전달하려는 데이터
			
			Send(new ArraySegment<byte>(sendBuff));
		}
		
		public override void OnConnected(EndPoint endPoint)
		{
			Console.WriteLine($"OnConnected : {endPoint}");

			{
				S_Connected connectedPacket = new S_Connected();
				Send(connectedPacket);
			}
		}

		public override void OnRecvPacket(ArraySegment<byte> buffer)
		{
			PacketManager.Instance.OnRecvPacket(this, buffer);
		}

		public override void OnDisconnected(EndPoint endPoint)
		{
			GameRoom room = RoomManager.Instance.Find(1);
			room.JobQ.Push(room.LeaveGame, MyPlayer.info.ObjectId);
			
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

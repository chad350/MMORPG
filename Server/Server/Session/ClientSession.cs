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
using static Google.Protobuf.Protocol.Person.Types;

namespace Server
{
	class ClientSession : PacketSession
	{
		public int SessionId { get; set; }

		public override void OnConnected(EndPoint endPoint)
		{
			Console.WriteLine($"OnConnected : {endPoint}");

			// PROTO Test
			S_Chat chat = new S_Chat()
			{
				Context = "안녕하세요"
			};

			
			ushort size = (ushort)chat.CalculateSize();
			byte[] sendBuff = new byte[size + 4]; 
			Array.Copy(BitConverter.GetBytes(size + 4), 0, sendBuff, 0, sizeof(ushort)); // 어느정도 크기의 데이터인지
			ushort protocolId = 2; 
			Array.Copy(BitConverter.GetBytes(protocolId), 0, sendBuff, 2, sizeof(ushort)); // 프로토콜의 아이디
			Array.Copy(chat.ToByteArray(), 0, sendBuff, 4, size); // 전달하려는 데이터
			
			Send(new ArraySegment<byte>(sendBuff));
		}

		public override void OnRecvPacket(ArraySegment<byte> buffer)
		{
			//PacketManager.Instance.OnRecvPacket(this, buffer);
		}

		public override void OnDisconnected(EndPoint endPoint)
		{
			SessionManager.Instance.Remove(this);

			Console.WriteLine($"OnDisconnected : {endPoint}");
		}

		public override void OnSend(int numOfBytes)
		{
			//Console.WriteLine($"Transferred bytes: {numOfBytes}");
		}
	}
}

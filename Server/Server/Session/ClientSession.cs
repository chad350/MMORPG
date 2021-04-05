﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;
using System.Net;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Game;

namespace Server
{
	public class ClientSession : PacketSession
	{
		// 현재 관리하고 있는 플레이어를 알면 코드 작성이 쉬워 진다.
		public Player MyPlayer { get; set; }
		public int SessionId { get; set; }

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
			
			// 원래는 입장준비가 끝났다고 클라이언트에서 판단 되면 패킷을 보내고
			// 해당 패킷을 받은다음 입장해야한다.
			MyPlayer = PlayerManager.Instance.Add();
			MyPlayer.info.Name = $"Player_{MyPlayer.info.PlayerId}";
			MyPlayer.info.PosInfo.State = CreatureState.Idle;
			MyPlayer.info.PosInfo.MoveDir = MoveDir.None;
			MyPlayer.info.PosInfo.PoxX = 0;
			MyPlayer.info.PosInfo.PoxY = 0;
				
			MyPlayer.Session = this;
			
			RoomManager.Instance.Find(1).EnterGame(MyPlayer);
		}

		public override void OnRecvPacket(ArraySegment<byte> buffer)
		{
			PacketManager.Instance.OnRecvPacket(this, buffer);
		}

		public override void OnDisconnected(EndPoint endPoint)
		{
			RoomManager.Instance.Find(1).LeaveGame(MyPlayer.info.PlayerId);
			
			SessionManager.Instance.Remove(this);

			Console.WriteLine($"OnDisconnected : {endPoint}");
		}

		public override void OnSend(int numOfBytes)
		{
			//Console.WriteLine($"Transferred bytes: {numOfBytes}");
		}
	}
}

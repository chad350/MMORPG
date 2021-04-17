using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Google.Protobuf;
using Google.Protobuf.Protocol;

public class ServerSession : PacketSession
{
    public int DummyId { get; set; }

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

    }

    public override void OnDisconnected(EndPoint endPoint)
    {

    }

    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        PacketManager.Instance.OnRecvPacket(this, buffer);
    }

    public override void OnSend(int numOfBytes)
    {
        //Console.WriteLine($"Transferred bytes: {numOfBytes}");
    }
}
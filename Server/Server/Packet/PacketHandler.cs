﻿using Server;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Google.Protobuf.Protocol;

class PacketHandler
{
	public static void C_ChatHandler(PacketSession session, IMessage packet)
	{
		S_Chat chatPacket = packet as S_Chat;
		ClientSession serverSession = session as ClientSession;

		Console.WriteLine(chatPacket.Context);
	}
}

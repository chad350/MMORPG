using System;
using System.Collections.Generic;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;

namespace Server.Game
{
    public partial class GameRoom
    {
        public void HandleEquipItem(Player player, C_EquipItem equipPacket)
        {
            if (player == null)
                return;

            player.HandleEquipItem(equipPacket);            
        }
    }
}
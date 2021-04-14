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

            Item item = player.Inven.Get(equipPacket.ItemDbId);
            if(item == null)
                return;
            
            // 굳이 DB 에 데이터 적용이 햇심적인 부분이 아니라 그냥 메모리 에 먼저 적용하고 DB 에는 알려만 줌
            // 메모리 선적용
            item.Equipped = equipPacket.Equipped;
            
            // DB 에 알려줌
            DbTransaction.EquipItemNoti(player, item);
            
            // 클라에 통보
            S_EquipItem equipOkItem = new S_EquipItem();
            equipOkItem.ItemDbId = equipPacket.ItemDbId;
            equipOkItem.Equipped = equipPacket.Equipped;
            player.Session.Send(equipOkItem);
        }
    }
}
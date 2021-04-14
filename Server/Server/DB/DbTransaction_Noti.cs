using System;
using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Game;
using Server.Utils;

namespace Server.DB
{
    // 그냥 요청만 하고 다시 접근 안할 요청들
    public partial class DbTransaction : JobSerializer
    {
        
        public static void EquipItemNoti(Player player, Item item)
        {
            if(player == null || item == null)
                return;

            ItemDb itemDb = new ItemDb()
            {
                ItemDbId = item.ItemDbId,
                Equipped = item.Equipped
            };
            
            // You (Db)
            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(itemDb).State = EntityState.Unchanged;
                    db.Entry(itemDb).Property(nameof(itemDb.Equipped)).IsModified = true;
 
                    bool success = db.SaveChangesEx();
                    if(success == false)
                    {
                        // 실패 행동
                    }
                }
            });
            
        }
    }
}
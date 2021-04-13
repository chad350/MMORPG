using System;
using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Game;
using Server.Utils;

namespace Server.DB
{
    public class DbTransaction : JobSerializer
    {
        public static DbTransaction Instance { get; } = new DbTransaction();

        #region 한번에 처리하는 방법
        // Me (GameRoom) -> You (Db) -> Me (GameRoom)
        public static void SavePlayerStatus_AllInOne(Player player, GameRoom room)
        {
            if(player == null || room == null)
                return;
            
            // Me (GameRoom)
            PlayerDb playerDb = new PlayerDb();
            playerDb.PlayerDbId = player.PlayerDbId;
            playerDb.Hp = player.Stat.Hp;
            
            // You (Db)
            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(playerDb).State = EntityState.Unchanged;
                    db.Entry(playerDb).Property(nameof(PlayerDb.Hp)).IsModified = true;
                    bool success = db.SaveChangesEx();
                    if(success)
                    {
                        // Me (GameRoom)
                        room.JobQ.Push(() => Console.WriteLine($"Hp Saved : {playerDb.Hp}"));
                    }
                }
            });
        }
        #endregion

        #region 스텝별로 나눠서 처리하는 방법
        // Me (GameRoom) -> You (Db)
        public static void SavePlayerStatus_Step1(Player player, GameRoom room)
        {
            if(player == null || room == null)
                return;
            
            // Me (GameRoom)
            PlayerDb playerDb = new PlayerDb();
            playerDb.PlayerDbId = player.PlayerDbId;
            playerDb.Hp = player.Stat.Hp;
            Instance.Push<PlayerDb, GameRoom>(SavePlayerStatus_Step2, playerDb, room);
        }

        // You (Db) -> Me (GameRoom)
        public static void SavePlayerStatus_Step2(PlayerDb playerDb, GameRoom room)
        {
            // You (Db)
            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(playerDb).State = EntityState.Unchanged;
                    db.Entry(playerDb).Property(nameof(PlayerDb.Hp)).IsModified = true;
                    bool success = db.SaveChangesEx();
                    if(success)
                    {
                        room.JobQ.Push(SavePlayerStatus_Step3, playerDb.Hp);
                    }
                }
            });
        }

        public static void SavePlayerStatus_Step3(int hp)
        {
            // Me (GameRoom)
            Console.WriteLine($"Hp Saved : {hp}");
        }
        #endregion

        public static void RewardPlayer(Player player, RewardData rewardData, GameRoom room)
        {
            if(player == null || rewardData == null || room == null)
                return;

            // 살짝 문제가 있긴하다.
            int? slot = player.Inven.GetEmptySlot();
            if(slot == null)
                return;
            
            ItemDb itemDb = new ItemDb()
            {
                TemplateId = rewardData.itemId,
                Count = rewardData.count,
                Slot = slot.Value,
                OwnerDbId = player.PlayerDbId
            };
            
            // You (Db)
            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Items.Add(itemDb);
                    bool success = db.SaveChangesEx();
                    if(success)
                    {
                        // Me (GameRoom)
                        room.JobQ.Push(() =>
                        {
                            Item newItem = Item.MakeItem(itemDb);
                            player.Inven.Add(newItem);
                            
                            // 클라이언트에게 전송
                            S_AddItem itemPacket = new S_AddItem();
                            ItemInfo itemInfo = new ItemInfo();
                            itemInfo.MergeFrom(newItem.Info);
                            itemPacket.Items.Add(itemInfo);
                            
                            player.Session.Send(itemPacket);
                        });
                    }
                }
            });
            
        }
    }
}
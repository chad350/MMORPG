using System;
using Microsoft.EntityFrameworkCore;
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
    }
}
using System;
using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.DB;
using Server.Utils;

namespace Server.Game
{
    public class Player : GameObject
    {
        public int PlayerDbId { get; set; }
        public ClientSession Session { get; set; }
        public Inventory Inven { get; private set; } = new Inventory();

        public Player()
        {
            ObjectType = GameObjectType.Player;
        }

        public override void OnDamaged(GameObject attacker, int damage)
        {
            base.OnDamaged(attacker, damage);
            
            
        }
        
        public override void OnDead(GameObject attacker)
        {
            base.OnDead(attacker);
        }

        public void OnLeaveGame()
        {
            // DB 연동?
            // 1) 피가 깍일 때마다 DB 접근 할 필요가 있을까?
            //  - DB 접근은 꽤 부하가 걸리는 작업이므로 최소화 할 필요가 있다.
            
            // 문제점
            // 1) 서버가 다운되면 아직 저장되지 않은 정보 날아감
            // 2) 코드 흐름을 다 막아버린다.
            //  - JobQ 가 일해야 되는데 굳이 DB 접근하면 그동안 일이 멈춰 있는다.
            //  - 그렇다면 해결 방법은
            //    a) 비동기로 실행한다?
            //    a) 다른 쓰레드로 DB 실행하게 던진다?
            //     - 결과를 받아서 이어 처리를 해야하는 경우 대응 못함
            //     - 결과를 받아서 이어 처리를 해야하는 경우 대응 못함
            
            // DbTransaction.cs 의 SavePlayerStatus_AllInOne 로 옮김
            // using (AppDbContext db = new AppDbContext())
            // {
            //     PlayerDb playerDb = new PlayerDb();
            //     playerDb.PlayerDbId = PlayerDbId;
            //     playerDb.Hp = Stat.Hp;
            //     
            //     db.Entry(playerDb).State = EntityState.Unchanged;
            //     db.Entry(playerDb).Property(nameof(PlayerDb.Hp)).IsModified = true;
            //     db.SaveChangesEx();
            // }
            
            DbTransaction.SavePlayerStatus_Step1(this, Room);
        }
    }
}
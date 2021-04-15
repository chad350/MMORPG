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

        public int WeaponDamage { get; private set; }
        public int ArmorDefence { get; private set; }

        public override int TotalAttack { get { return Stat.Attack + WeaponDamage; } }
        public override int TotalDefence { get { return ArmorDefence; } }
        
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

        public void HandleEquipItem(C_EquipItem equipPacket)
        {
            Item item = Inven.Get(equipPacket.ItemDbId);
            if(item == null)
                return;
            
            if(item.ItemType == ItemType.Consumable)
                return;

            // 착용 요청일 때 기본 착용 아이템 해제 (같은 부위)
            if (equipPacket.Equipped)
            {
                Item unequipItem = null;
                // 무기라면
                if (item.ItemType == ItemType.Weapon)
                {
                    // 무기이며 장착중인 아이템
                    unequipItem = Inven.Find(i => i.Equipped && i.ItemType == ItemType.Weapon);
                }
                else if(item.ItemType == ItemType.Armor)
                {
                    // 방어구이며 장착중이고 부위가 일치하는 아이템 - ex) Armor, Helmet
                    ArmorType armorType = ((Armor) item).ArmorType;
                    unequipItem = Inven.Find(i => i.Equipped && i.ItemType == ItemType.Armor && ((Armor) item).ArmorType == armorType);
                }

                if (unequipItem != null)
                {
                    unequipItem.Equipped = false;
            
                    // DB 에 알려줌
                    DbTransaction.EquipItemNoti(this, unequipItem);
            
                    // 클라에 통보
                    S_EquipItem equipOkItem = new S_EquipItem();
                    equipOkItem.ItemDbId = unequipItem.ItemDbId;
                    equipOkItem.Equipped = unequipItem.Equipped;
                    Session.Send(equipOkItem);
                }
            }

            {
                // 굳이 DB 에 데이터 적용이 햇심적인 부분이 아니라 그냥 메모리 에 먼저 적용하고 DB 에는 알려만 줌
                // 메모리 선적용
                item.Equipped = equipPacket.Equipped;
            
                // DB 에 알려줌
                DbTransaction.EquipItemNoti(this, item);
            
                // 클라에 통보
                S_EquipItem equipOkItem = new S_EquipItem();
                equipOkItem.ItemDbId = equipPacket.ItemDbId;
                equipOkItem.Equipped = equipPacket.Equipped;
                Session.Send(equipOkItem);
            }

            RefreshAdditionalStat();
        }

        public void RefreshAdditionalStat()
        {
            WeaponDamage = 0;
            ArmorDefence = 0;

            foreach (Item item in Inven.Items.Values)
            {
                if(item.Equipped == false)
                    continue;

                switch (item.ItemType)
                {
                    case ItemType.Weapon:
                        WeaponDamage += ((Weapon) item).Damage;
                        break;
                    
                    case ItemType.Armor:
                        ArmorDefence += ((Armor) item).Defence;
                        break;
                }
            }
        }
    }
}
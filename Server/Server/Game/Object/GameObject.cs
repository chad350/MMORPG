using System;
using Google.Protobuf.Protocol;
using Server.Data;

namespace Server.Game
{
    public class GameObject
    {
        public GameObjectType ObjectType { get; protected set; } = GameObjectType.None;

        public int Id
        {
            get { return info.ObjectId; }
            set { info.ObjectId = value; }
        }

        public GameRoom Room { get; set; }

        public ObjectInfo info { get; set; } = new ObjectInfo();
        public PositionInfo PosInfo { get; private set; } = new PositionInfo();
        public StatInfo Stat { get; private set; } = new StatInfo();

        public virtual int TotalAttack { get { return Stat.Attack; } }
        public virtual int TotalDefence { get { return 0; } }

        public float Speed
        {
            get { return Stat.Speed; }
            set { Stat.Speed = value; }
        }
        
        public int Hp
        {
            get { return Stat.Hp; }
            set { Stat.Hp = Math.Clamp(value, 0, Stat.MaxHp); }
        }

        public MoveDir Dir
        {
            get { return PosInfo.MoveDir; }
            set { PosInfo.MoveDir = value; }
        }

        public CreatureState State
        {
            get { return PosInfo.State;}
            set { PosInfo.State = value; }
        }

        public GameObject()
        {
            info.PosInfo = PosInfo;
            info.StatInfo = Stat;
        }

        public virtual void Update()
        {
            
        }

        public Vector2Int CellPos
        {
            get
            {
                return new Vector2Int(PosInfo.PoxX, PosInfo.PoxY);
            }
            set
            {
                PosInfo.PoxX = value.x;
                PosInfo.PoxY = value.y;
            }
        }

        public Vector2Int GetFrontCellPos()
        {
            return GetFrontCellPos(PosInfo.MoveDir);
        }

        public Vector2Int GetFrontCellPos(MoveDir dir)
        {
            Vector2Int cellPos = CellPos;
            
            switch (dir)
            {
                case MoveDir.Up:
                    cellPos += Vector2Int.up;
                    break;
                case MoveDir.Down:
                    cellPos += Vector2Int.down;
                    break;
                case MoveDir.Left:
                    cellPos += Vector2Int.left;
                    break;
                case MoveDir.Right:
                    cellPos += Vector2Int.right;
                    break;
            }

            return cellPos;
        }

        public static MoveDir GetDirFromVector(Vector2Int dir)
        {
            if (dir.x > 0)
                return MoveDir.Right;
            else if (dir.x < 0)
                return MoveDir.Left;
            else if (dir.y > 0)
                return MoveDir.Up;
            else
                return MoveDir.Down;
        }
        
        public virtual void OnDamaged(GameObject attacker, int damage)
        {
            if(Room == null)
                return;
            
            damage = Math.Max(damage - TotalDefence, 0);
            Stat.Hp = Math.Max(Stat.Hp - damage, 0);

            S_ChangeHp changePacket = new S_ChangeHp();
            changePacket.ObjectId = Id;
            changePacket.Hp = Stat.Hp;
            Room.Broadcast(CellPos, changePacket);
            
            if (Stat.Hp <= 0)
                OnDead(attacker);
        }

        public virtual void OnDead(GameObject attacker)
        {
            if(Room == null)
                return;
            
            S_Die diePacket = new S_Die();
            diePacket.ObjectId = Id;
            diePacket.AttackerId = attacker.Id;
            Room.Broadcast(CellPos, diePacket);

            // ?????? LeaveGame, EnterGame ??? ???????????? ????????? ???????????? ??????????????? ????????? ??????????????? ??????????????? ???????????? ?????????
            // ????????? ????????? ???????????? ?????? ????????? ?????? ?????? ??????.
            // ?????? ????????? ?????? JobQ??? ?????? ?????? ?????? ???????????? ?????? ??????
            // ??? ????????? ????????? ????????? OnDead ??? ???????????? ????????? JobQ?????? ?????? ?????? ??????
            // ?????? JobQ?????? ?????? ?????? ?????? ????????? ?????? ???????????? ????????? ?????? ?????????.
            GameRoom room = Room;
            room.LeaveGame(Id); // ??? ??????

            Stat.Hp = Stat.MaxHp;
            PosInfo.State = CreatureState.Idle;
            PosInfo.MoveDir = MoveDir.Down;

            room.EnterGame(this, randomPos:true); // ??? ??????
        }

        public virtual GameObject GetOwner()
        {
            return this;
        }
    }
}
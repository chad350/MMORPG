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

        public float Speed
        {
            get { return Stat.Speed; }
            set { Stat.Speed = value; }
        }

        public GameObject()
        {
            info.PosInfo = PosInfo;
            info.StatInfo = Stat;
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

        public virtual void OnDamaged(GameObject attacker, int damage)
        {
            
        }
    }
}
using System.Collections.Generic;
using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class ObjectManager
    {
        public static ObjectManager Instance { get; } = new ObjectManager();

        private object _lock = new object();
        private Dictionary<int, Player> _players = new Dictionary<int, Player>();
        
        // int32 를 다 쓸일은 없으니
        // 비트 플래그처럼 쪼개서 사용하는 경우가 많음
        // 32비트 중 앞의 8비트를 쪼개서 타입을 알려주는데 사용
        // 맨 앞은 보통 부호를 나타내니 뒤의 7비트만 사용 - 127가지의 숫자
        // [ UNUSED(1) ] [ TYPE(7) ] [ ID(24) ]
        // [........ | ........ | ........ | ........]
        private int _counter = 0;

        public T Add<T>() where T : GameObject, new()
        {
            T gameObject = new T();

            lock (_lock)
            {
                gameObject.Id = GenerateId(gameObject.ObjectType);

                if (gameObject.ObjectType == GameObjectType.Player)
                {
                    _players.Add(gameObject.Id, gameObject as Player);
                }
            }

            return gameObject;
        }

        int GenerateId(GameObjectType type)
        {
            lock (_lock)
            {
                return ((int) type << 24) | (_counter++);
            }
        }
        
        public static GameObjectType GetObjectTypeById(int id)
        {
            int type = (id >> 24) & 0x7F;
            return (GameObjectType) type;
        }

        public bool Remove(int objectId)
        {
            GameObjectType objectType = GetObjectTypeById(objectId);
            lock (_lock)
            {
                if(objectType == GameObjectType.Player)
                    return _players.Remove(objectId);
            }

            return false;
        }

        public Player Find(int objectId)
        {
            GameObjectType objectType = GetObjectTypeById(objectId);
            lock (_lock)
            {
                if (objectType == GameObjectType.Player)
                {
                    Player player = null;
                    if(_players.TryGetValue(objectId, out player))
                        return player;
                }
            }
            
            return null;
        }
    }
}
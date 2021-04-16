using System.Collections.Generic;

namespace Server.Game
{
    public class GameLogic
    {
        public JobSerializer JobQ = new JobSerializer();
        public static GameLogic Instance { get; } = new GameLogic();
        
        private Dictionary<int, GameRoom> _rooms = new Dictionary<int, GameRoom>();
        private int _roomId = 1;

        public void Update()
        {
            JobQ.Flush();
            
            // Update 는 꽤나 부하가 큰작업
            // Lock 을 걸면 처리에 시간이 걸리니 병목현상이 심해 질 수 있다.
            // 1. Lock 을 없애고 Add,Remove 를 서버가 뜨는 최초에만 실행하는 방법
            // 2. JobQ 를 이용해 순차적으로 실행하는 방법
            foreach (var room in _rooms.Values)
            {
                room.Update();
            }
        }

        public GameRoom Add(int mapId)
        {
            GameRoom gameRoom = new GameRoom();
            gameRoom.JobQ.Push(gameRoom.Init, mapId);
            
            gameRoom.RoomId = _roomId;
            _rooms.Add(_roomId, gameRoom);
            _roomId++;

            return gameRoom;
        }

        public bool Remove(int roomId)
        {
            return _rooms.Remove(roomId);
        }

        public GameRoom Find(int roomId)
        {
            GameRoom room = null;
            if(_rooms.TryGetValue(roomId, out room))
                return room;
            return null;
        }
    }
}
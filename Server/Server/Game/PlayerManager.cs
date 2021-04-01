using System.Collections.Generic;

namespace Server.Game
{
    public class PlayerManager
    {
        public static PlayerManager Instance { get; } = new PlayerManager();

        private object _lock = new object();
        private Dictionary<int, Player> _players = new Dictionary<int, Player>();
        
        // int32 를 다 쓸일은 없으니
        // 비트 플래그처럼 쪼개서 사용하는 경우가 많음
        private int _playerId;

        public Player Add()
        {
            Player player = new Player();

            lock (_lock)
            {
                player.info.PlayerId = _playerId;
                _players.Add(_playerId, player);
                _playerId++;
            }

            return player;
        }

        public bool Remove(int playerId)
        {
            lock (_lock)
            {
                return _players.Remove(playerId);
            }
        }

        public Player Find(int playerId)
        {
            lock (_lock)
            {
                Player player = null;
                if(_players.TryGetValue(playerId, out player))
                    return player;
                return null;
            }
        }
    }
}
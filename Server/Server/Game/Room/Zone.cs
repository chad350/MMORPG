using System;
using System.Collections.Generic;

namespace Server.Game
{
    public class Zone
    {
        public int IndexY { get; private set; }
        public int IndexX { get; private set; }

        public HashSet<Player> Players { get; set; } = new HashSet<Player>();
        public HashSet<Monster> Monsters { get; set; } = new HashSet<Monster>();
        public HashSet<Projectile> Projectiles { get; set; } = new HashSet<Projectile>();

        public Zone(int y, int x)
        {
            IndexY = y;
            IndexX = x;
        }

        public Player FindOnePlayer(Func<Player, bool> condition)
        {
            foreach (var player in Players)
            {
                if (condition.Invoke(player))
                    return player;
            }

            return null;
        }
        
        public List<Player> FindAllPlayer(Func<Player, bool> condition)
        {
            List<Player> findList = new List<Player>();
            
            foreach (var player in Players)
            {
                if (condition.Invoke(player))
                    findList.Add(player);
            }
            
            return findList;
        }
    }
}
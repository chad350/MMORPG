using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;

namespace Server.Game
{
    public partial class GameRoom
    {
        // 잡에 푸시해서 사용하는 경우는 리턴값을 받지 않는다.
        public JobSerializer JobQ = new JobSerializer();
        public int RoomId { get; set; }

        private Dictionary<int, Player> _players = new Dictionary<int, Player>();
        private Dictionary<int, Monster> _monsters = new Dictionary<int, Monster>();
        private Dictionary<int, Projectile> _projectiles = new Dictionary<int, Projectile>();

        public Zone[,] Zones { get; private set; }
        public int ZoneCells { get; private set; }

        public Map Map { get; private set; } = new Map();

        public Zone GetZone(Vector2Int cellPos)
        {
            int x = (cellPos.x - Map.MinX) / ZoneCells;
            int y = (Map.MaxY - cellPos.y) / ZoneCells;

            // 2중 배열의 GetLength(int dimension)
            // dimension : 0 = 1차원  Zones[y, x] : y의 크기
            // dimension : 1 = 2차원  Zones[y, x] : x의 크기
            if (x < 0 || x >= Zones.GetLength(1))
                return null;
            if (y < 0 || y >= Zones.GetLength(0))
                return null;
            
            return Zones[y, x];
        }

        public void Init(int mapId, int zoneCells)
        {
            Map.LoadMap(mapId);

            // Zone Init
            ZoneCells = zoneCells; // 10
            // 1 ~ 10 칸 = 1 존
            // 11 ~ 20칸 = 2존
            // 21 ~ 30칸 = 3존
            int countY = (Map.SizeY + zoneCells - 1) / zoneCells;
            int countX = (Map.SizeX + zoneCells - 1) / zoneCells;
            Zones = new Zone[countY, countX];
            for (int y = 0; y < countY; y++)
            {
                for (int x = 0; x < countX; x++)
                {
                    Zones[y, x] = new Zone(y, x);
                }
            }
            
            Monster monster = ObjectManager.Instance.Add<Monster>();
            monster.Init(1);
            
            monster.CellPos = new Vector2Int(0, 5);
            JobQ.Push(EnterGame, monster);
        }
        
        public void Update()
        {
            JobQ.Flush();
        }

        public void EnterGame(GameObject gameObject)
        {
            if(gameObject == null)
                return;

            GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);

            if (type == GameObjectType.Player)
            {
                Player player = gameObject as Player;
                _players.Add(gameObject.Id, player);
                player.Room = this;

                player.RefreshAdditionalStat();
                
                Map.ApplyMove(player, new Vector2Int(player.CellPos.x, player.CellPos.y));
                GetZone(player.CellPos).Players.Add(player);
                
                // 본인한테 정보 전송
                {
                    S_EnterGame enterPacket = new S_EnterGame();
                    enterPacket.Player = player.info;
                    player.Session.Send(enterPacket);
            
                    S_Spawn spawnPacket = new S_Spawn();
                    foreach (Player p in _players.Values)
                    {
                        if(player != p)
                            spawnPacket.Objects.Add(p.info);
                    }
                    
                    foreach (Monster m in _monsters.Values)
                        spawnPacket.Objects.Add(m.info);
                    
                    foreach (Projectile p in _projectiles.Values)
                        spawnPacket.Objects.Add(p.info);

                    player.Session.Send(spawnPacket);
                }
            }
            else if (type == GameObjectType.Monster)
            {
                Monster monster = gameObject as Monster;
                _monsters.Add(gameObject.Id, monster);
                monster.Room = this;
                
                Map.ApplyMove(monster, new Vector2Int(monster.CellPos.x, monster.CellPos.y));
                
                monster.Update();
            }
            else if (type == GameObjectType.Projectile)
            {
                Projectile projectile = gameObject as Projectile;
                _projectiles.Add(gameObject.Id, projectile);
                projectile.Room = this;

                projectile.Update();
            }


            // 타인한테 정보 전송
            {
                S_Spawn spawnPacket = new S_Spawn();
                spawnPacket.Objects.Add(gameObject.info);
                foreach (Player p in _players.Values)
                {
                    if(gameObject.Id != p.Id)
                        p.Session.Send(spawnPacket);
                }
            }
        }
        
        public void LeaveGame(int objectId)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(objectId);
            
            if (type == GameObjectType.Player)
            {
                Player player = null;
                if(_players.Remove(objectId, out player) == false)
                    return;
                
                GetZone(player.CellPos).Players.Remove(player);
                
                player.OnLeaveGame();
                Map.ApplyLeave(player);
                player.Room = null;
                
                // 본인한테 정보 전송
                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    player.Session.Send(leavePacket);
                }
            }
            else if (type == GameObjectType.Monster)
            {
                Monster monster = null;
                if(_monsters.Remove(objectId, out monster) == false)
                    return;
            
                Map.ApplyLeave(monster);
                monster.Room = null;
            }
            else if (type == GameObjectType.Projectile)
            {
                Projectile projectile = null;
                if(_projectiles.Remove(objectId, out projectile) == false)
                    return;
            
                projectile.Room = null;
            }
            
            
            // 타인한테 정보 전송
            {
                S_Despawn despawnPacket = new S_Despawn();
                despawnPacket.ObjectIds.Add(objectId);
                foreach (Player p in _players.Values)
                {
                    if(objectId != p.Id)
                        p.Session.Send(despawnPacket);
                }
            }
        }

        // Update 에서 실행되는 함수
        // Update는 이미 JobQ 에서 관리 되기 때문에 이 부분은 잡큐에 등록하지 않아도 된다.
        // 만약 호출하는 곳에서도 JobQ 에 있지 않다면 문제가 생길것
        public Player FindPlayer(Func<GameObject, bool> condition)
        {
            foreach (Player player in _players.Values)
            {
                if (condition.Invoke(player))
                    return player;
            }

            return null;
        }

        // 마찬가지로 Broadcast를 호출되는 부분은 다 JobQ로 관리되는 곳에서 호출한다. 
        public void Broadcast(Vector2Int pos, IMessage packet)
        {
            List<Zone> zones = GetAdjacentZones(pos);
            // foreach (var zone in zones)
            // {
            //     foreach (Player p in zone.Players)
            //     {
            //         p.Session.Send(packet);
            //     }
            // }

            foreach (Player p in zones.SelectMany(z => z.Players))
            {
                // Send 는 부하가 상당히 큰 작업
                // 데이터를 전송할 떄 컨텍스트 스위칭이 일어난다.
                p.Session.Send(packet);
            }
        }

        public List<Zone> GetAdjacentZones(Vector2Int cellPos, int cells = 5)
        {
            HashSet<Zone> zones = new HashSet<Zone>();
            int[] delta = new int[2] {-cells, +cells};
            foreach (int dy in delta)
            {
                foreach (int dx in delta)
                {
                    int y = cellPos.y + dy;
                    int x = cellPos.x + dx;
                    Zone zone = GetZone(new Vector2Int(x, y));
                    if(zone == null)
                        continue;
                    zones.Add(zone);
                }   
            }

            return zones.ToList();
        }
    }
}
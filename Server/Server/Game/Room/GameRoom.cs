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

        public const int VisionCells = 5;
        
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
            return GetZone(y, x);
        }

        public Zone GetZone(int indexY, int indexX)
        {
            // 2중 배열의 GetLength(int dimension)
            // dimension : 0 = 1차원  Zones[y, x] : y의 크기
            // dimension : 1 = 2차원  Zones[y, x] : x의 크기
            if (indexY < 0 || indexX >= Zones.GetLength(1))
                return null;
            if (indexX < 0 || indexY >= Zones.GetLength(0))
                return null;
            
            return Zones[indexY, indexX];
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

            for (int i = 0; i < 500; i++)
            {
                Monster monster = ObjectManager.Instance.Add<Monster>();
                monster.Init(1);
                EnterGame(monster);   
            }
        }
        
        public void Update()
        {
            JobQ.Flush();
        }

        private Random _rand = new Random();
        public void EnterGame(GameObject gameObject, bool randomPos = true)
        {
            if(gameObject == null)
                return;

            if (randomPos)
            {
                Vector2Int respawnPos;
                while (true)
                {
                    respawnPos.x = _rand.Next(Map.MinX, Map.MaxX + 1);
                    respawnPos.y = _rand.Next(Map.MinY, Map.MaxY + 1);
                    if (Map.Find(respawnPos) == null)
                    {
                        gameObject.CellPos = respawnPos;
                        break;
                    }
                }
            }


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
            
                    player.Vision.Update();
                }
            }
            else if (type == GameObjectType.Monster)
            {
                Monster monster = gameObject as Monster;
                _monsters.Add(gameObject.Id, monster);
                monster.Room = this;
                
                GetZone(monster.CellPos).Monsters.Add(monster);
                Map.ApplyMove(monster, new Vector2Int(monster.CellPos.x, monster.CellPos.y));
                
                monster.Update();
            }
            else if (type == GameObjectType.Projectile)
            {
                Projectile projectile = gameObject as Projectile;
                _projectiles.Add(gameObject.Id, projectile);
                projectile.Room = this;

                GetZone(projectile.CellPos).Projectiles.Add(projectile);
                projectile.Update();
            }
            
            // 타인에게 정보 전송
            {
                S_Spawn spawnPacket = new S_Spawn();
                spawnPacket.Objects.Add(gameObject.info);
                Broadcast(gameObject.CellPos, spawnPacket);
            }
        }
        
        public void LeaveGame(int objectId)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(objectId);

            Vector2Int cellPos;
            if (type == GameObjectType.Player)
            {
                Player player = null;
                if(_players.Remove(objectId, out player) == false)
                    return;

                cellPos = player.CellPos;

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
            
                cellPos = monster.CellPos;

                Map.ApplyLeave(monster);
                monster.Room = null;
            }
            else if (type == GameObjectType.Projectile)
            {
                Projectile projectile = null;
                if(_projectiles.Remove(objectId, out projectile) == false)
                    return;
            
                cellPos = projectile.CellPos;
                Map.ApplyLeave(projectile);
                projectile.Room = null;
            }
            else
            {
                return;
            }
            
            // 타인에게 정보 전송
            {
                S_Despawn despawnPacket = new S_Despawn();
                despawnPacket.ObjectIds.Add(objectId);
                Broadcast(cellPos, despawnPacket);
            }
        }

        // Update 에서 실행되는 함수
        // Update는 이미 JobQ 에서 관리 되기 때문에 이 부분은 잡큐에 등록하지 않아도 된다.
        // 만약 호출하는 곳에서도 JobQ 에 있지 않다면 문제가 생길것
        Player FindPlayer(Func<GameObject, bool> condition)
        {
            foreach (Player player in _players.Values)
            {
                if (condition.Invoke(player))
                    return player;
            }

            return null;
        }
        
        // 부하가 걸리는 함수
        public Player FindClosestPlayer(Vector2Int pos, int range)
        {
            List<Player> players = GetAdjacentPlayer(pos, range);
            
            players.Sort((left, right) =>
            {
                int leftDist = (left.CellPos - pos).cellDistFromZero;
                int rightDist = (right.CellPos - pos).cellDistFromZero;
                return leftDist - rightDist;
            });

            foreach (var player in players)
            {
                List<Vector2Int> path = Map.FindPath(pos, player.CellPos, true);
                if (path.Count < 2 || path.Count > range)
                    continue;
                
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
                int dx = p.CellPos.x - pos.x;
                int dy = p.CellPos.y - pos.y;
                if(Math.Abs(dx) > GameRoom.VisionCells)
                    continue;
                if(Math.Abs(dy) > GameRoom.VisionCells)
                    continue;
                
                // Send 는 부하가 상당히 큰 작업
                // 데이터를 전송할 떄 컨텍스트 스위칭이 일어난다.
                p.Session.Send(packet);
            }
        }

        public List<Player> GetAdjacentPlayer(Vector2Int pos, int range)
        {
            List<Zone> zones = GetAdjacentZones(pos, range);
            return zones.SelectMany(z => z.Players).ToList();
        }

        public List<Zone> GetAdjacentZones(Vector2Int cellPos, int range = VisionCells)
        {
            HashSet<Zone> zones = new HashSet<Zone>();

            int maxY = cellPos.y + range;
            int minY = cellPos.y - range;
            int maxX = cellPos.x + range;
            int minX = cellPos.x - range;
            
            // 좌측 상단
            Vector2Int leftTop = new Vector2Int(minX, maxY);
            int minIndexY = (Map.MaxY - leftTop.y) / ZoneCells;
            int minIndexX = (leftTop.x - Map.MinX) / ZoneCells;

            // 우측 하단
            Vector2Int rightBot = new Vector2Int(maxX, minY);
            int maxIndexY = (Map.MaxY - rightBot.y) / ZoneCells;
            int maxIndexX = (rightBot.x - Map.MinX) / ZoneCells;

            for (int x = minIndexX; x <= maxIndexX; x++)
            {
                for (int y = minIndexY; y <= maxIndexY; y++)
                {
                    Zone zone = GetZone(y, x);
                    if(zone == null)
                        continue;

                    zones.Add(zone);
                }
            }
            
            int[] delta = new int[2] {-range, +range};
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
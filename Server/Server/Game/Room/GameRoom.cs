using System;
using System.Collections.Generic;
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

        public Map Map { get; private set; } = new Map();

        public void Init(int mapId)
        {
            Map.LoadMap(mapId);

            Monster monster = ObjectManager.Instance.Add<Monster>();
            monster.Init(1);
            
            monster.CellPos = new Vector2Int(0, 5);
            JobQ.Push(EnterGame, monster);
        }
        
        public void Update()
        {
            foreach (Monster monster in _monsters.Values)
            {
                monster.Update();
            }
                
            foreach (Projectile projectile in _projectiles.Values)
            {
                projectile.Update();
            }
            
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

                Map.ApplyMove(player, new Vector2Int(player.CellPos.x, player.CellPos.y));
                
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
            }
            else if (type == GameObjectType.Projectile)
            {
                Projectile projectile = gameObject as Projectile;
                _projectiles.Add(gameObject.Id, projectile);
                projectile.Room = this;
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
        public void Broadcast(IMessage packet)
        {
            foreach (Player p in _players.Values)
                p.Session.Send(packet);
        }
    }
}
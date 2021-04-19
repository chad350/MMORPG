using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore.Metadata.Builders.Internal;

namespace Server.Game
{
    public class VisionCube
    {
        public Player Owner { get; private set; }
        public HashSet<GameObject> PreviousObjects { get; private set; } = new HashSet<GameObject>();

        public VisionCube(Player owner)
        {
            Owner = owner;
        }

        public HashSet<GameObject> GatherObjects()
        {
            if (Owner == null || Owner.Room == null)
                return null;

            HashSet<GameObject> objects = new HashSet<GameObject>();
            
            Vector2Int cellPos = Owner.CellPos;
            List<Zone> zones = Owner.Room.GetAdjacentZones(cellPos);

            foreach (var zone in zones)
            {
                foreach (var player in zone.Players)
                {
                    int dx = player.CellPos.x - cellPos.x;
                    int dy = player.CellPos.y - cellPos.y;
                    if(Math.Abs(dx) > GameRoom.VisionCells)
                        continue;
                    if(Math.Abs(dy) > GameRoom.VisionCells)
                        continue;
                    objects.Add(player);
                }
                
                foreach (var monster in zone.Monsters)
                {
                    int dx = monster.CellPos.x - cellPos.x;
                    int dy = monster.CellPos.y - cellPos.y;
                    if(Math.Abs(dx) > GameRoom.VisionCells)
                        continue;
                    if(Math.Abs(dy) > GameRoom.VisionCells)
                        continue;
                    objects.Add(monster);
                }
                
                foreach (var projectile in zone.Projectiles)
                {
                    int dx = projectile.CellPos.x - cellPos.x;
                    int dy = projectile.CellPos.y - cellPos.y;
                    if(Math.Abs(dx) > GameRoom.VisionCells)
                        continue;
                    if(Math.Abs(dy) > GameRoom.VisionCells)
                        continue;
                    objects.Add(projectile);
                }
            }
            
            return objects;
        }

        public void Update()
        {
            if(Owner == null || Owner.Room == null)
                return;

            HashSet<GameObject> currentObjects = GatherObjects();

            // 기존엔 없었는데 새로 생긴 객체 Spawn
            List<GameObject> added = currentObjects.Except(PreviousObjects).ToList();
            if (added.Count > 0)
            {
                S_Spawn spawnPacket = new S_Spawn();
                foreach (var gameObject in added)
                {
                    ObjectInfo info = new ObjectInfo();
                    info.MergeFrom(gameObject.info);
                    spawnPacket.Objects.Add(info);
                }
                
                Owner.Session.Send(spawnPacket);
            }
            
            // 기존엔 있었는데 사라진 객체 Despawn
            List<GameObject> removed = PreviousObjects.Except(currentObjects).ToList();
            if (removed.Count > 0)
            {
                S_Despawn despawnPacket = new S_Despawn();
                foreach (var gameObject in removed)
                {
                    despawnPacket.ObjectIds.Add(gameObject.Id);
                }
                
                Owner.Session.Send(despawnPacket);
            }

            PreviousObjects = currentObjects;
            
            Owner.Room.JobQ.PushAfter(500, Update);
        }
    }
}
using System;
using System.Collections.Generic;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;

namespace Server.Game
{
    public class GameRoom
    {
        private object _lock = new object();
        public int RoomId { get; set; }

        private Dictionary<int, Player> _players = new Dictionary<int, Player>();
        private Dictionary<int, Monster> _monsters = new Dictionary<int, Monster>();
        private Dictionary<int, Projectile> _projectiles = new Dictionary<int, Projectile>();

        public Map Map { get; private set; } = new Map();

        public void Init(int mapId)
        {
            Map.LoadMap(mapId);
        }

        public void Update()
        {
            lock (_lock)
            {
                foreach (Projectile projectile in _projectiles.Values)
                {
                    projectile.Update();
                }
            }
        }

        public void EnterGame(GameObject gameObject)
        {
            if(gameObject == null)
                return;

            GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);
            
            lock (_lock)
            {
                if (type == GameObjectType.Player)
                {
                    Player player = gameObject as Player;
                    _players.Add(gameObject.Id, player);
                    player.Room = this;
                
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
                        player.Session.Send(spawnPacket);
                    }
                }
                else if (type == GameObjectType.Monster)
                {
                    Monster monster = gameObject as Monster;
                    _monsters.Add(gameObject.Id, monster);
                    monster.Room = this;
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
        }
        
        public void LeaveGame(int objectId)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(objectId);
            
            lock (_lock)
            {
                if (type == GameObjectType.Player)
                {
                    Player player = null;
                    if(_players.Remove(objectId, out player) == false)
                        return;
                
                    player.Room = null;
                    Map.ApplyLeave(player);
                    
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
                
                    monster.Room = null;
                    Map.ApplyLeave(monster);
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
        }

        public void HandleMove(Player player, C_Move movePacket)
        {
            if (player == null)
                return;

            lock (_lock)
            {
                // 검증 단계 - 클라는 거짓말을 하기 때문
                PositionInfo movePosInfo = movePacket.PosInfo; // 이동 예정 정보
                ObjectInfo info = player.info; // 실제 플레이어 정보

                // 다른 좌표로 이동할 경우 갈 수 있는지 체크
                // 지금 플레이어의 정보와 이동할 정보가 다르다면 -> 이동할 거라면
                if (movePosInfo.PoxX != info.PosInfo.PoxX || movePosInfo.PoxY != info.PosInfo.PoxY)
                {
                    // 이동할 정보 (movePosInfo) 로 갈 수 있는지 체크 
                    if(Map.CanGo(new Vector2Int(movePosInfo.PoxX, movePosInfo.PoxY)) == false)
                        return; // 못하면 리턴
                }
                // 좌표가 아닌 방향등 다른 정보를 위한 패킷이었으면 통과

                // 통과 됬으면 정보 업데이트
                info.PosInfo.State = movePosInfo.State;
                info.PosInfo.MoveDir = movePosInfo.MoveDir;
                Map.ApplyMove(player, new Vector2Int(movePosInfo.PoxX, movePosInfo.PoxY));
                
                // 다른 플레이어에게 브로드캐스트
                S_Move resMovePacket = new S_Move();
                resMovePacket.ObjectId = player.info.ObjectId;
                resMovePacket.PosInfo = movePacket.PosInfo;
		
                Broadcast(resMovePacket);   
            }
        }

        public void HandleSkill(Player player, C_Skill skillPacket)
        {
            if (player == null)
                return;

            lock (_lock)
            {
                ObjectInfo info = player.info;
                // 스킬을 쓸수 있는 상태인지 체크 - 클라는 믿을수 없다.
                if(info.PosInfo.State != CreatureState.Idle)
                    return;
                
                // 스킬 사용 가능 여부 체크

                //스킬을 사용한다는 애니메이션 동기화 
                info.PosInfo.State = CreatureState.Skill;
                S_Skill skill = new S_Skill() {Info = new SkillInfo()};
                skill.ObjectId = info.ObjectId;
                skill.Info.SkillId = skillPacket.Info.SkillId;
                Broadcast(skill);

                Skill skillData = null;
                if(DataManager.SkillDict.TryGetValue(skillPacket.Info.SkillId, out skillData) == false)
                    return;

                switch (skillData.skillType)
                {
                    case SkillType.SkillAuto:
                        // 데미지 판정
                        Vector2Int skillPos = player.GetFrontCellPos(info.PosInfo.MoveDir);
                        GameObject target = Map.Find(skillPos);
                
                        if (target != null)
                        {
                            Console.WriteLine($"Hit GameObject - {target.ObjectType} !");
                        }
                        break;
                    
                    case SkillType.SkillProjectile:
                        Arrow arrow = ObjectManager.Instance.Add<Arrow>();
                        if(arrow == null)
                            return;

                        arrow.Owner = player;
                        arrow.Data = skillData;
                        arrow.PosInfo.State = CreatureState.Moving;
                        arrow.PosInfo.MoveDir = player.PosInfo.MoveDir;
                        arrow.PosInfo.PoxX = player.PosInfo.PoxX;
                        arrow.PosInfo.PoxY = player.PosInfo.PoxY;
                        arrow.Speed = skillData.projectile.speed;
                        
                        EnterGame(arrow);
                        break;
                }
            }
        }
        

        public void Broadcast(IMessage packet)
        {
            lock (_lock)
            {
                foreach (Player p in _players.Values)
                    p.Session.Send(packet);
            }
        }
    }
}
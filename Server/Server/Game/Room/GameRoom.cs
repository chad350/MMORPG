using System;
using System.Collections.Generic;
using Google.Protobuf;
using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class GameRoom
    {
        private object _lock = new object();
        public int RoomId { get; set; }

        private Dictionary<int, Player> _players = new Dictionary<int,Player>();

        private Map _map = new Map();

        public void Init(int mapId)
        {
            _map.LoadMap(mapId);
        }

        public void EnterGame(Player newPlayer)
        {
            if(newPlayer == null)
                return;

            lock (_lock)
            {
                _players.Add(newPlayer.info.ObjectId, newPlayer);
                newPlayer.Room = this;
                
                // 본인한테 정보 전송
                {
                    S_EnterGame enterPacket = new S_EnterGame();
                    enterPacket.Player = newPlayer.info;
                    newPlayer.Session.Send(enterPacket);
                
                    S_Spawn spawnPacket = new S_Spawn();
                    foreach (Player p in _players.Values)
                    {
                        if(newPlayer != p)
                            spawnPacket.Objects.Add(p.info);
                    }
                    newPlayer.Session.Send(spawnPacket);
                }

                // 타인한테 정보 전송
                {
                    S_Spawn spawnPacket = new S_Spawn();
                    spawnPacket.Objects.Add(newPlayer.info);
                    foreach (Player p in _players.Values)
                    {
                        if(newPlayer != p)
                            p.Session.Send(spawnPacket);
                    }
                }
            }
        }
        
        public void LeaveGame(int playerId)
        {
            lock (_lock)
            {
                Player player = null;
                if(_players.Remove(playerId, out player) == false)
                    return;
                
                player.Room = null;

                // 본인한테 정보 전송
                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    player.Session.Send(leavePacket);
                }
                
                // 타인한테 정보 전송
                {
                    S_Despawn despawnPacket = new S_Despawn();
                    despawnPacket.PlayerIds.Add(player.info.ObjectId);
                    foreach (Player p in _players.Values)
                    {
                        if(player != p)
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
                    if(_map.CanGo(new Vector2Int(movePosInfo.PoxX, movePosInfo.PoxY)) == false)
                        return; // 못하면 리턴
                }
                // 좌표가 아닌 방향등 다른 정보를 위한 패킷이었으면 통과

                // 통과 됬으면 정보 업데이트
                info.PosInfo.State = movePosInfo.State;
                info.PosInfo.MoveDir = movePosInfo.MoveDir;
                _map.ApplyMove(player, new Vector2Int(movePosInfo.PoxX, movePosInfo.PoxY));
                
                // 다른 플레이어에게 브로드캐스트
                S_Move resMovePacket = new S_Move();
                resMovePacket.PlayerId = player.info.ObjectId;
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
                skill.PlayerId = info.ObjectId;
                skill.Info.SkillId = skillPacket.Info.SkillId;
                Broadcast(skill);
                
                if (skillPacket.Info.SkillId == 1)
                {
                    // 데미지 판정
                    Vector2Int skillPos = player.GetFrontCellPos(info.PosInfo.MoveDir);
                    Player target = _map.Find(skillPos);
                
                    if (target != null)
                    {
                        Console.WriteLine("Hit Player !");
                    }
                }
                else if(skillPacket.Info.SkillId == 2)
                {
                    
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
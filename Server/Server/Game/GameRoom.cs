using System.Collections.Generic;
using Google.Protobuf;
using Google.Protobuf.Protocol;

namespace Server.Game
{
    public class GameRoom
    {
        private object _lock = new object();
        public int RoomId { get; set; }

        private List<Player> _players = new List<Player>();

        public void EnterGame(Player newPlayer)
        {
            if(newPlayer == null)
                return;

            lock (_lock)
            {
                _players.Add(newPlayer);
                newPlayer.Room = this;
                
                // 본인한테 정보 전송
                {
                    S_EnterGame enterPacket = new S_EnterGame();
                    enterPacket.Player = newPlayer.info;
                    newPlayer.Session.Send(enterPacket);
                
                    S_Spawn spawnPacket = new S_Spawn();
                    foreach (Player p in _players)
                    {
                        if(newPlayer != p)
                            spawnPacket.Players.Add(p.info);
                    }
                    newPlayer.Session.Send(spawnPacket);
                }

                // 타인한테 정보 전송
                {
                    S_Spawn spawnPacket = new S_Spawn();
                    spawnPacket.Players.Add(newPlayer.info);
                    foreach (Player p in _players)
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
                Player player = _players.Find(p => p.info.PlayerId == playerId);
                if(player == null)
                    return;

                _players.Remove(player);
                player.Room = null;
                
                // 본인한테 정보 전송
                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    player.Session.Send(leavePacket);
                }
                
                // 타인한테 정보 전송
                {
                    S_Despawn despawnPacket = new S_Despawn();
                    despawnPacket.PlayerIds.Add(player.info.PlayerId);
                    foreach (Player p in _players)
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
            
            
                // 서버에서 좌표 이동
                PlayerInfo info = player.info;
                info.PosInfo = movePacket.PosInfo;
		
                // 다른 플레이어에게 브로드캐스트
                S_Move resMovePacket = new S_Move();
                resMovePacket.PlayerId = player.info.PlayerId;
                resMovePacket.PosInfo = movePacket.PosInfo;
		
                Broadcast(resMovePacket);   
            }
        }

        public void Broadcast(IMessage packet)
        {
            lock (_lock)
            {
                foreach (Player p in _players)
                    p.Session.Send(packet);
            }
        }
    }
}
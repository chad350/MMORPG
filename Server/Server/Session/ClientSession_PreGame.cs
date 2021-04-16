using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DB;
using Server.Game;
using Server.Utils;
using ServerCore;

namespace Server
{
    public partial class ClientSession : PacketSession
    {
        public int AccountDbId { get; private set; }
        public List<LobbyPlayerInfo> lobbyPlayers { get; set; } = new List<LobbyPlayerInfo>();

        public void HandleLogin(C_Login loginPacket)
        {
            // 보안 체크
            if(ServerState != PlayerServerState.ServerStateLogin)
                return;
            
            lobbyPlayers.Clear();
            
            // 문제가 있긴 하다.
            // 1. db 를 여기서 접근하는게 맞는가
            // 2. 보안문제
            //	- 동시에 여러 사람이 같은 UniqueId를 보낸다면
            //	- 악의적으로 여러번 보낸다면
            //	- 의도한 시점 이외에 이 패킷을 보낸다면
            using (AppDbContext db = new AppDbContext())
            {
                AccountDb findAccount = db.Accounts
                    .Include(a => a.Players)
                    .Where(a => a.AccountName == loginPacket.UniqueId)
                    .FirstOrDefault();
			
                if(findAccount != null)
                {
                    // AccountDbId 메모리에 기억
                    AccountDbId = findAccount.AccountDbId;
                    
                    // 찾은 계정이 있다면 로그인 
                    S_Login loginOk = new S_Login() { LoginOk = 1 };
                    foreach (PlayerDb playerDb in findAccount.Players)
                    {
                        LobbyPlayerInfo lobbyPlayer = new LobbyPlayerInfo()
                        {
                            PlayerDbId = playerDb.PlayerDbId,
                            Name =  playerDb.PlayerName,
                            StatInfo = new StatInfo()
                            {
                                Level =  playerDb.Level,
                                Hp =  playerDb.Hp,
                                MaxHp =  playerDb.MaxHp,
                                Attack =  playerDb.Attack,
                                Speed =  playerDb.Speed,
                                TotalExp =  playerDb.TotalExp,
                                
                            }
                        };
                        
                        // 메모리에도 들고
                        lobbyPlayers.Add(lobbyPlayer);
                        
                        // 패킷에 넣어준다
                        loginOk.Players.Add(lobbyPlayer);
                    }
                    
                    Send(loginOk);
                    
                    // 로비로 이동
                    ServerState = PlayerServerState.ServerStateLobby;
                }
                else
                {
                    // 찾은 계정이 없다면 계정 생성 후 로그인 
                    AccountDb newAccount = new AccountDb() {AccountName = loginPacket.UniqueId};
                    db.Accounts.Add(newAccount);
                    db.SaveChangesEx(); // 실패 시 예외처리
				
                    // AccountDbId 메모리에 기억
                    AccountDbId = newAccount.AccountDbId;
                    
                    S_Login loginOk = new S_Login() { LoginOk = 1 };
                    Send(loginOk);
                    
                    // 로비로 이동
                    ServerState = PlayerServerState.ServerStateLobby;
                }
            }
        }

        public void HandleCreatePlayer(C_CreatePlayer createPacket)
        {
            if(ServerState != PlayerServerState.ServerStateLobby)
                return;

            using (AppDbContext db = new AppDbContext())
            {
                PlayerDb findPlayer = db.Players
                    .Where(p => p.PlayerName == createPacket.Name)
                    .FirstOrDefault();

                if (findPlayer != null)
                {
                    // 이름이 겹친다.
                    Send(new S_CreatePlayer());
                }
                else
                {
                    // 1레벨 스탯 정보 추출
                    StatInfo stat = null;
                    DataManager.StatDict.TryGetValue(1, out stat);
                    
                    // DB에 플레이어 만들어 줘야 함
                    PlayerDb newPlayerDb = new PlayerDb()
                    {
                        PlayerName = createPacket.Name,
                        Level =  stat.Level,
                        Hp = stat.Hp,
                        MaxHp = stat.MaxHp,
                        Attack = stat.Attack,
                        Speed = stat.Speed,
                        TotalExp = 0,
                        AccountDbId = AccountDbId
                    };

                    db.Players.Add(newPlayerDb);
                    db.SaveChangesEx(); // 예외처리
                    
                    // 메모리에 추가
                    LobbyPlayerInfo lobbyPlayer = new LobbyPlayerInfo()
                    {
                        PlayerDbId = newPlayerDb.PlayerDbId,
                        Name =  createPacket.Name,
                        StatInfo = new StatInfo()
                        {
                            Level =  stat.Level,
                            Hp =  stat.Hp,
                            MaxHp =  stat.MaxHp,
                            Attack =  stat.Attack,
                            Speed =  stat.Speed,
                            TotalExp =  0
                                
                        }
                    };
                        
                    // 메모리에도 들고
                    lobbyPlayers.Add(lobbyPlayer);
                    
                    // 클라에 전송
                    S_CreatePlayer newPlayer = new S_CreatePlayer() { Player = new LobbyPlayerInfo()};
                    newPlayer.Player.MergeFrom(lobbyPlayer);
                    
                    Send(newPlayer);
                }
            }
        }

        public void HandleEnterGame(C_EnterGame enterGamePacket)
        {
            if(ServerState != PlayerServerState.ServerStateLobby)
                return;

            LobbyPlayerInfo playerInfo = lobbyPlayers.Find(p => p.Name == enterGamePacket.Name);
            if(playerInfo == null)
                return;
            
            // 원래는 입장준비가 끝났다고 클라이언트에서 판단 되면 패킷을 보내고
            // 해당 패킷을 받은다음 입장해야한다.
            MyPlayer = ObjectManager.Instance.Add<Player>();
            MyPlayer.PlayerDbId = playerInfo.PlayerDbId;
            MyPlayer.info.Name = playerInfo.Name;
            MyPlayer.info.PosInfo.State = CreatureState.Idle;
            MyPlayer.info.PosInfo.MoveDir = MoveDir.Down;
            MyPlayer.info.PosInfo.PoxX = 0;
            MyPlayer.info.PosInfo.PoxY = 0;
            MyPlayer.Stat.MergeFrom(playerInfo.StatInfo);
            MyPlayer.Session = this;

            S_ItemList itemListPacket = new S_ItemList();
            // 아이템 목록을 가지고 온다
            using (AppDbContext db = new AppDbContext())
            {
                List<ItemDb> items = db.Items
                    .Where(i => i.OwnerDbId == playerInfo.PlayerDbId)
                    .ToList();

                // 인벤토리
                foreach (ItemDb itemDb in items)
                {
                    Item item = Item.MakeItem(itemDb);
                    if (item != null)
                    {
                        MyPlayer.Inven.Add(item);
                        
                        ItemInfo info = new ItemInfo();
                        info.MergeFrom(item.Info);
                        itemListPacket.Items.Add(info);
                    }
                }
            }
            
            // 클라에게 아이템 목록 전달
            Send(itemListPacket);
            
            ServerState = PlayerServerState.ServerStateGame;
            
            GameLogic.Instance.JobQ.Push(() =>
            {
                GameRoom room = GameLogic.Instance.Find(1);
                room.JobQ.Push(room.EnterGame, MyPlayer); 
            });
        }
    }
}
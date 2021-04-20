using System;
using System.Collections.Generic;
using System.Linq;
using AccountServer.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedDB;

namespace AccountServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private AppDbContext _context;
        private SharedDbContext _shared;

        public AccountController(AppDbContext context, SharedDbContext shared)
        {
            _context = context;
            _shared = shared;
        }

        [HttpPost]
        [Route("create")]
        public CreateAccountPacketRecv CreateAccount([FromBody] CreateAccountPacketReq req)
        {
            CreateAccountPacketRecv recv = new CreateAccountPacketRecv();

            AccountDb account =  _context.Accounts
                                    .AsNoTracking()
                                    .Where(a => a.AccountName == req.AccountName)
                                    .FirstOrDefault();

            if (account == null)
            {
                _context.Accounts.Add(new AccountDb()
                {
                    AccountName =  req.AccountName,
                    Password = req.Password
                });

                bool success = _context.SaveChangesEx();
                recv.CreateOk = success;
            }
            else
            {
                recv.CreateOk = false;
            }
            
            return recv;
        }

        [HttpPost]
        [Route("login")]
        public LoginAccountPacketRecv LoginAccount([FromBody] LoginAccountPacketReq req)
        {
            LoginAccountPacketRecv recv = new LoginAccountPacketRecv();

            AccountDb account =  _context.Accounts
                                    .AsNoTracking()
                                    .Where(a => a.AccountName == req.AccountName && a.Password == req.Password)
                                    .FirstOrDefault();

            if (account == null)
            {
                recv.LoginOk = false;
            }
            else
            {
                recv.LoginOk = true;
                // 토큰 발급
                DateTime expired = DateTime.UtcNow;
                expired.AddSeconds(600);

                // 토큰이 이미 있는지 체크
                TokenDb tokenDb = _shared.Tokens.Where(t => t.AccountDbId == account.AccountDbId).FirstOrDefault();
                if (tokenDb != null)
                {
                    tokenDb.Token = new Random().Next(Int32.MinValue, Int32.MaxValue);
                    tokenDb.Expired = expired;
                    _shared.SaveChangesEx();
                }
                else
                {
                    tokenDb = new TokenDb()
                    {
                        AccountDbId = account.AccountDbId,
                        Token = new Random().Next(Int32.MinValue, Int32.MaxValue),
                        Expired = expired
                    };
                    _shared.Add(tokenDb);
                    _shared.SaveChangesEx();
                }

                recv.AccountId = account.AccountDbId;
                recv.Token = tokenDb.Token;
                recv.ServerList = new List<ServerInfo>();

                foreach (ServerDb serverDb in _shared.Servers)
                {
                    recv.ServerList.Add(new ServerInfo()
                    {
                        Name = serverDb.Name,
                        IpAddress = serverDb.IpAddress,
                        Port = serverDb.Port,
                        BusyScore = serverDb.BusyScore
                    });
                }
                
                // recv.ServerList = new List<ServerInfo>()
                // {
                //     new ServerInfo() {Name = "302", IpAddress = "127.0.0.1", BusyScore = 0},
                //     new ServerInfo() {Name = "salin", IpAddress = "127.0.0.1", BusyScore = 3}
                // };
            }
            
            return recv;
        }
    }
}
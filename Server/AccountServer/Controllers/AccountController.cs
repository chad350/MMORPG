using System.Collections.Generic;
using System.Linq;
using AccountServer.DB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AccountServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
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

                recv.ServerList = new List<ServerInfo>()
                {
                    new ServerInfo() {Name = "302", Ip = "127.0.0.1", CrowdedLevel = 0},
                    new ServerInfo() {Name = "salin", Ip = "127.0.0.1", CrowdedLevel = 3}
                };
            }
            
            return recv;
        }
    }
}
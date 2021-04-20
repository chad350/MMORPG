using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharedDB
{
    // 비대칭 키를 이용하면 좀 더 안전하게 테이터를 주고 받을 수 있다.
    [Table("Token")]
    public class TokenDb
    {
        public int TokenDbId { get; set; }
        public int AccountDbId { get; set; }
        public int Token { get; set; }
        public DateTime Expired { get; set; }
    }

    // 레디스면 좀더 빠르게 동작 할 수 있다.
    [Table("ServerInfo")]
    public class ServerDb
    {
        public int ServerDbId { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public int BusyScore { get; set; }
    }
}
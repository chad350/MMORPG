using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Server.Data;

namespace Server.DB
{
    public class AppDbContext : DbContext
    {
        public DbSet<AccountDb> Accounts { get; set; }
        public DbSet<PlayerDb> Players { get; set; }

        private static readonly ILoggerFactory _logger = LoggerFactory.Create(builder => builder.AddConsole());
        
        private string _connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=GameDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseLoggerFactory(_logger)
                .UseSqlServer(ConfigManager.Config == null ? _connectionString : ConfigManager.Config.connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 계정과 플레이어 정보에 이름을 인덱스로 설정
            // 이후에 이름으로 찾을 일이 있을 확률이 높기 때문
            // 또한 유니크하게 설정
            modelBuilder.Entity<AccountDb>()
                .HasIndex(a => a.AccountName)
                .IsUnique();
            
            modelBuilder.Entity<PlayerDb>()
                .HasIndex(p => p.PlayerName)
                .IsUnique();
        }
    }
}
using Microsoft.EntityFrameworkCore;

namespace SharedDB
{
    public class SharedDbContext : DbContext
    {
        public DbSet<TokenDb> Tokens { get; set; }
        public DbSet<ServerDb> Servers { get; set; }

        // for GameServer
        public SharedDbContext()
        {
            
        }
        
        // for  ASP.NET
        public SharedDbContext(DbContextOptions<SharedDbContext> options) : base(options)
        {
            
        }
        
        // for GameServer 
        public static string ConnectionString { get; set; } = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SharedDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // ASP.NET 에서는 options에 이미 세팅되어 있다.
            if (optionsBuilder.IsConfigured == false)
            {
                optionsBuilder
                    //.UseLoggerFactory(_logger)
                    .UseSqlServer(ConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TokenDb>()
                .HasIndex(t => t.AccountDbId)
                .IsUnique();
            
            modelBuilder.Entity<ServerDb>()
                .HasIndex(s => s.Name)
                .IsUnique();
        }
    }
}
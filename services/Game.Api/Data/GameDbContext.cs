using Microsoft.EntityFrameworkCore;

namespace Game.Api.Data
{
    public class GameDbContext : DbContext
    {
        public GameDbContext(DbContextOptions<GameDbContext> options)
            : base(options)
        {
        }

        public DbSet<GameSession> GameSessions { get; set; }
        public DbSet<Word> Words { get; set; }
        public DbSet<PlayerSettings> PlayerSettings { get; set; }
    }
}

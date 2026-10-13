using Microsoft.EntityFrameworkCore;

namespace Leaderboard.Api.Data;

public class LeaderboardDbContext : DbContext
{
    public LeaderboardDbContext(DbContextOptions<LeaderboardDbContext> options)
        : base(options)
    {
    }

    public DbSet<HighScore> HighScores => Set<HighScore>();
}

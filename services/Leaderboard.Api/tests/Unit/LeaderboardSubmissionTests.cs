using Leaderboard.Api.Data;
using Leaderboard.Api.Models;
using Leaderboard.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Leaderboard.Api.Tests;

public class LeaderboardSubmissionTests
{
    [Fact]
    public async Task SubmitScoreAsync_NewPlayer_InsertsScoreAndQualifies()
    {
        await using var db = CreateDbContext();
        var service = new LeaderboardService(db);

        var qualifies = await service.SubmitScoreAsync(CreateRequest("  Player  ", 12));

        var score = await db.HighScores.SingleAsync();
        Assert.True(qualifies);
        Assert.Equal("Player", score.PlayerName);
        Assert.Equal(12, score.Score);
        Assert.Equal("english", score.Language);
        Assert.Equal("beginner", score.Difficulty);
        Assert.Equal("car-parts", score.Topic);
        Assert.NotEqual(default, score.Date);
    }

    [Fact]
    public async Task SubmitScoreAsync_HigherScore_UpdatesExistingPlayer()
    {
        await using var db = CreateDbContext();
        var originalDate = DateTime.UtcNow.AddDays(-1);
        db.HighScores.Add(CreateEntity("Player", 5, originalDate));
        await db.SaveChangesAsync();
        var service = new LeaderboardService(db);

        var qualifies = await service.SubmitScoreAsync(CreateRequest("Player", 8));

        var score = await db.HighScores.SingleAsync();
        Assert.True(qualifies);
        Assert.Equal(8, score.Score);
        Assert.True(score.Date > originalDate);
    }

    [Fact]
    public async Task SubmitScoreAsync_PlayerNameMatchingIsCaseInsensitive()
    {
        await using var db = CreateDbContext();
        db.HighScores.Add(CreateEntity("HNK", 3));
        await db.SaveChangesAsync();
        var service = new LeaderboardService(db);

        var qualifies = await service.SubmitScoreAsync(CreateRequest("hnk", 4));

        var scores = await db.HighScores.ToListAsync();
        Assert.True(qualifies);
        Assert.Single(scores);
        Assert.Equal("HNK", scores[0].PlayerName);
        Assert.Equal(4, scores[0].Score);
    }

    [Fact]
    public async Task SubmitScoreAsync_LowerScore_DoesNotUpdateOrQualify()
    {
        await using var db = CreateDbContext();
        var originalDate = DateTime.UtcNow.AddDays(-1);
        db.HighScores.Add(CreateEntity("Player", 8, originalDate));
        await db.SaveChangesAsync();
        var service = new LeaderboardService(db);

        var qualifies = await service.SubmitScoreAsync(CreateRequest("Player", 5));

        var score = await db.HighScores.SingleAsync();
        Assert.False(qualifies);
        Assert.Equal(8, score.Score);
        Assert.Equal(originalDate, score.Date);
    }

    [Fact]
    public async Task SubmitScoreAsync_NewScore_PrunesLowestScoreOutsideTopTen()
    {
        await using var db = CreateDbContext();
        db.HighScores.AddRange(Enumerable.Range(1, 10)
            .Select(score => CreateEntity($"Player{score}", score)));
        await db.SaveChangesAsync();
        var service = new LeaderboardService(db);

        var qualifies = await service.SubmitScoreAsync(CreateRequest("NewPlayer", 11));

        var scores = await db.HighScores.OrderByDescending(x => x.Score).ToListAsync();
        Assert.True(qualifies);
        Assert.Equal(10, scores.Count);
        Assert.Equal(11, scores[0].Score);
        Assert.DoesNotContain(scores, x => x.PlayerName == "Player1");
    }

    [Fact]
    public async Task SubmitScoreAsync_NewScoreBelowTopTen_DoesNotQualifyOrRemain()
    {
        await using var db = CreateDbContext();
        db.HighScores.AddRange(Enumerable.Range(1, 10)
            .Select(score => CreateEntity($"Player{score}", score)));
        await db.SaveChangesAsync();
        var service = new LeaderboardService(db);

        var qualifies = await service.SubmitScoreAsync(CreateRequest("NewPlayer", 0));

        Assert.False(qualifies);
        Assert.Equal(10, await db.HighScores.CountAsync());
        Assert.DoesNotContain(await db.HighScores.ToListAsync(), x => x.PlayerName == "NewPlayer");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public async Task SubmitScoreAsync_InvalidPlayerName_ReturnsFalse(string playerName)
    {
        await using var db = CreateDbContext();
        var service = new LeaderboardService(db);

        var qualifies = await service.SubmitScoreAsync(CreateRequest(playerName, 10));

        Assert.False(qualifies);
        Assert.Empty(await db.HighScores.ToListAsync());
    }

    [Fact]
    public async Task SubmitScoreAsync_SamePlayerInDifferentCategories_CreatesSeparateScores()
    {
        await using var db = CreateDbContext();
        var service = new LeaderboardService(db);

        Assert.True(await service.SubmitScoreAsync(CreateRequest("Player", 5)));
        Assert.True(await service.SubmitScoreAsync(CreateRequest("Player", 6, language: "dutch")));
        Assert.True(await service.SubmitScoreAsync(CreateRequest("Player", 7, difficulty: "intermediate")));
        Assert.True(await service.SubmitScoreAsync(CreateRequest("Player", 8, topic: "beverages")));

        Assert.Equal(4, await db.HighScores.CountAsync());
    }

    private static LeaderboardDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<LeaderboardDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new LeaderboardDbContext(options);
    }

    private static SubmitScoreRequest CreateRequest(
        string playerName,
        int score,
        string language = "english",
        string difficulty = "beginner",
        string topic = "car-parts")
    {
        return new SubmitScoreRequest
        {
            PlayerName = playerName,
            Score = score,
            Language = language,
            Difficulty = difficulty,
            Topic = topic
        };
    }

    private static HighScore CreateEntity(
        string playerName,
        int score,
        DateTime? date = null)
    {
        return new HighScore
        {
            PlayerName = playerName,
            Score = score,
            Language = "english",
            Difficulty = "beginner",
            Topic = "car-parts",
            Date = date ?? DateTime.UtcNow.AddMinutes(-score)
        };
    }
}

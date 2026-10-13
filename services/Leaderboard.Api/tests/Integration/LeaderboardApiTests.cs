using System.Net;
using System.Net.Http.Json;
using Leaderboard.Api.Data;
using Leaderboard.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Leaderboard.Api.Tests;

public class LeaderboardApiTests
{
    [Fact]
    public async Task SubmitAndGetScore_UsesDtoAndReturnsQualification()
    {
        using var factory = new LeaderboardApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/leaderboard/submit", new
        {
            id = 99,
            playerName = "  Player  ",
            score = 12,
            language = "ENGLISH",
            difficulty = "Beginner",
            topic = "Car-Parts",
            date = "2000-01-01T00:00:00Z"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<SubmissionResponse>();
        Assert.NotNull(result);
        Assert.True(result.Qualifies);

        var scores = await client.GetFromJsonAsync<List<HighScore>>(
            "/api/leaderboard/english/beginner/car-parts");
        var score = Assert.Single(scores!);
        Assert.Equal(1, score.Id);
        Assert.Equal("Player", score.PlayerName);
        Assert.Equal(12, score.Score);
        Assert.True(score.Date > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task SubmitScore_RepeatedPlayerKeepsBestScoreAndSingleEntry()
    {
        using var factory = new LeaderboardApiFactory();
        using var client = factory.CreateClient();

        var first = await SubmitScoreAsync(client, "Player", 15);
        var lower = await SubmitScoreAsync(client, " Player ", 9);
        var higher = await SubmitScoreAsync(client, "Player", 20);

        Assert.True(first.Qualifies);
        Assert.False(lower.Qualifies);
        Assert.True(higher.Qualifies);

        var scores = await client.GetFromJsonAsync<List<HighScore>>(
            "/api/leaderboard/english/beginner/car-parts");
        var score = Assert.Single(scores!);
        Assert.Equal(20, score.Score);
    }

    [Fact]
    public async Task SubmitScore_PrunesEntriesOutsideTopTen()
    {
        using var factory = new LeaderboardApiFactory();
        using var client = factory.CreateClient();

        for (var score = 1; score <= 10; score++)
        {
            var response = await SubmitScoreAsync(client, $"Player{score}", score);
            Assert.True(response.Qualifies);
        }

        var qualifies = await SubmitScoreAsync(client, "NewPlayer", 11);
        var scores = await client.GetFromJsonAsync<List<HighScore>>(
            "/api/leaderboard/english/beginner/car-parts");

        Assert.True(qualifies.Qualifies);
        Assert.Equal(10, scores!.Count);
        Assert.Equal(11, scores[0].Score);
        Assert.DoesNotContain(scores, score => score.PlayerName == "Player1");
    }

    [Fact]
    public async Task SubmitScore_BlankPlayerName_DoesNotPersistOrQualify()
    {
        using var factory = new LeaderboardApiFactory();
        using var client = factory.CreateClient();

        var response = await SubmitScoreAsync(client, " ", 12);
        var scores = await client.GetFromJsonAsync<List<HighScore>>(
            "/api/leaderboard/english/beginner/car-parts");

        Assert.False(response.Qualifies);
        Assert.Empty(scores!);
    }

    private static async Task<SubmissionResponse> SubmitScoreAsync(
        HttpClient client,
        string playerName,
        int score)
    {
        var response = await client.PostAsJsonAsync("/api/leaderboard/submit", new SubmitScoreRequest
        {
            PlayerName = playerName,
            Score = score,
            Language = "english",
            Difficulty = "beginner",
            Topic = "car-parts"
        });
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<SubmissionResponse>();
        return Assert.IsType<SubmissionResponse>(result);
    }

    private sealed class SubmissionResponse
    {
        public bool Qualifies { get; set; }
    }

    private sealed class LeaderboardApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<LeaderboardDbContext>();
                services.RemoveAll<DbContextOptions<LeaderboardDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<LeaderboardDbContext>>();
                services.AddDbContext<LeaderboardDbContext>(options =>
                    options.UseInMemoryDatabase(_databaseName));
            });
        }
    }
}

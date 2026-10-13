using Leaderboard.Api.Data;
using Leaderboard.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Leaderboard.Api.Services;

public class LeaderboardService : ILeaderboardService
{
    private readonly LeaderboardDbContext _db;

    public LeaderboardService(LeaderboardDbContext db)
    {
        _db = db;
    }

    public async Task<bool> SubmitScoreAsync(SubmitScoreRequest request)
    {
        var playerName = request.PlayerName?.Trim();
        if (string.IsNullOrWhiteSpace(playerName))
        {
            return false;
        }

        var language = request.Language.Trim().ToLowerInvariant();
        var difficulty = request.Difficulty.Trim().ToLowerInvariant();
        var topic = request.Topic.Trim().ToLowerInvariant();

        var scores = await _db.HighScores
            .Where(x => x.Language == language &&
                        x.Difficulty == difficulty &&
                        x.Topic == topic)
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Date)
            .ThenBy(x => x.Id)
            .ToListAsync();

        var matchingScores = scores.Where(x => x.PlayerName == playerName).ToList();
        var existing = matchingScores.FirstOrDefault();
        var duplicateScores = matchingScores.Skip(1).ToList();
        if (duplicateScores.Count > 0)
        {
            _db.HighScores.RemoveRange(duplicateScores);
            scores.RemoveAll(duplicateScores.Contains);
        }

        HighScore candidate;
        var scoreImproved = false;

        if (existing is not null)
        {
            candidate = existing;
            if (request.Score > existing.Score)
            {
                existing.Score = request.Score;
                existing.Date = DateTime.UtcNow;
                scoreImproved = true;
            }
        }
        else
        {
            candidate = new HighScore
            {
                PlayerName = playerName,
                Score = request.Score,
                Language = language,
                Difficulty = difficulty,
                Topic = topic,
                Date = DateTime.UtcNow
            };

            scores.Add(candidate);
            _db.HighScores.Add(candidate);
            scoreImproved = true;
        }

        var topScores = scores
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Date)
            .ThenBy(x => x.Id)
            .Take(10)
            .ToList();

        var qualifies = scoreImproved && topScores.Contains(candidate);
        var toRemove = scores.Where(x => !topScores.Contains(x)).ToList();

        if (toRemove.Count > 0)
        {
            _db.HighScores.RemoveRange(toRemove);
        }

        await _db.SaveChangesAsync();
        return qualifies;
    }

    public async Task<List<HighScore>> GetTopScoresAsync(string language, string difficulty, string topic)
    {
        language = language.Trim().ToLowerInvariant();
        difficulty = difficulty.Trim().ToLowerInvariant();
        topic = topic.Trim().ToLowerInvariant();

        return await _db.HighScores
            .Where(h => h.Language == language && h.Difficulty == difficulty && h.Topic == topic)
            .OrderByDescending(h => h.Score)
            .ThenBy(h => h.Date)
            .ThenBy(h => h.Id)
            .Take(10)
            .ToListAsync();
    }

public async Task<bool> RemovePlayerScoresAsync(string playerName)
{
    var trimmedName = playerName?.Trim();
    if (string.IsNullOrWhiteSpace(trimmedName)) return false;

    // 🔒 STRICT TESTING NAMING GUARD
    // Only allow deletion if the player name starts with our explicit automation testing prefixes
    var upperName = trimmedName.ToUpperInvariant();
    if (!upperName.StartsWith("TEST_") && !upperName.StartsWith("E2E_"))
    {
        return false; 
    }

    var entriesToRemove = await _db.HighScores
        .Where(x => x.PlayerName.ToLower() == trimmedName.ToLower())
        .ToListAsync();

    if (entriesToRemove.Count == 0) return false;

    _db.HighScores.RemoveRange(entriesToRemove);
    await _db.SaveChangesAsync();
    return true;
}


}

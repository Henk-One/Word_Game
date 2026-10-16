using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Desktop.Game.Api;
using Desktop.Game.Models;

namespace Desktop.Game.Services;

// UI-vriendelijk datamodel voor de Highscore Grid-representatie
public sealed record ViewHighScoreRecord(string Rank, string PlayerName, int Score, string FormattedDate);

// =========================================================================
// MODULE 1: DATA & TELEMETRY SERVICE (High Cohesion & Loose Coupling)
// Handles pure business logic computation and asynchronous API conversions.
// =========================================================================
public class DesktopLeaderboardService
{
    private readonly LeaderboardApiClient _leaderboardApiClient;

    private readonly string[] _highscoreLanguages = ["english", "dutch", "indonesian"];
    private readonly string[] _highscoreDifficulties = ["beginner", "intermediate"];
    private readonly string[] _highscoreTopics = ["beverages", "car-parts"];

    public DesktopLeaderboardService(LeaderboardApiClient leaderboardApiClient)
    {
        _leaderboardApiClient = leaderboardApiClient;
    }

    /// <summary>
    /// Fetches raw data from the server and transforms it into view-ready, structured logic records.
    /// Satisfies: "Everything concerning the screen should be separate from data fetching."
    /// </summary>
    public async Task<List<ViewHighScoreRecord>> GetCleanLeaderboardAsync(string language, string difficulty, string topic)
    {
        var rawScores = await _leaderboardApiClient.GetAsync(language, difficulty, topic);

        return rawScores.Take(10).Select((score, index) => new ViewHighScoreRecord(
            Rank: $"#{index + 1}",
            PlayerName: score.PlayerName,
            Score: score.Score,
            FormattedDate: score.Date == DateTime.MinValue ? "Unavailable" : score.Date.ToLocalTime().ToString("g")
        )).ToList();
    }

    /// <summary>
    /// Utility helper to translate raw string preferences into a zero-based flat collection index map.
    /// Kept within the data layer to satisfy High Cohesion.
    /// </summary>
    public int ComputeHighscoreIndex(string language, string difficulty, string topic)
    {
        var languageIndex = Array.IndexOf(_highscoreLanguages, language);
        var difficultyIndex = Array.IndexOf(_highscoreDifficulties, difficulty);
        var topicIndex = Array.IndexOf(_highscoreTopics, topic);

        int languageCount = _highscoreDifficulties.Length * _highscoreTopics.Length;
        return Math.Max(0, languageIndex * languageCount + difficultyIndex * _highscoreTopics.Length + topicIndex);
    }

    public string GetMetadataString(int index)
    {
        var languageCount = _highscoreDifficulties.Length * _highscoreTopics.Length;
        var language = _highscoreLanguages[index / languageCount];
        var remainder = index % languageCount;
        var difficulty = _highscoreDifficulties[remainder / _highscoreTopics.Length];
        var topic = _highscoreTopics[remainder % _highscoreTopics.Length];

        return $"Mode: {language.ToUpperInvariant()} / {difficulty.ToUpperInvariant()} / {topic.ToUpperInvariant()}";
    }

    public int GetTotalCategoryCount()
    {
        return _highscoreLanguages.Length * _highscoreDifficulties.Length * _highscoreTopics.Length;
    }
}


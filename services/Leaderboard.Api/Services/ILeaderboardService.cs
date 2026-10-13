using Leaderboard.Api.Models;
using Leaderboard.Api.Data;

namespace Leaderboard.Api.Services;

public interface ILeaderboardService
{
    Task<bool> SubmitScoreAsync(SubmitScoreRequest request);
    Task<List<HighScore>> GetTopScoresAsync(string language, string difficulty, string topic);
    Task<bool> RemovePlayerScoresAsync(string playerName); // New method
}


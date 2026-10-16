using System.Net.Http;
using System.Net.Http.Json;
using Desktop.Game.Models;

namespace Desktop.Game.Api;

public sealed class LeaderboardApiClient(HttpClient httpClient)
{
    private const string BaseUrl = "http://localhost:5000/api/leaderboard";

    public async Task<IReadOnlyList<HighScoreResponse>> GetAsync(string language, string difficulty, string topic)
    {
        var url = $"{BaseUrl}/{Uri.EscapeDataString(language)}/{Uri.EscapeDataString(difficulty)}/{Uri.EscapeDataString(topic)}";
        return await httpClient.GetFromJsonAsync<List<HighScoreResponse>>(url) ?? [];
    }
}

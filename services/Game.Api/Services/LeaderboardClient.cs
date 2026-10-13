using System.Net.Http.Json;

namespace Game.Api.Services;

public class LeaderboardClient
{
    private readonly HttpClient _http;

    public LeaderboardClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<bool> SubmitScoreAsync(string playerName, int score, string language, string difficulty, string topic)
    {
        var payload = new
        {
            playerName,
            score,
            language,
            difficulty,
            topic
        };

        var response = await _http.PostAsJsonAsync("api/leaderboard/submit", payload);
        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        var result = await response.Content.ReadFromJsonAsync<SubmitScoreResponse>();
        return result?.Qualifies ?? false;
    }
}

public class SubmitScoreResponse
{
    public bool Qualifies { get; set; }
}

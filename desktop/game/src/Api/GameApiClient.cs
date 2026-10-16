using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Desktop.Game.Models;

namespace Desktop.Game.Api;

public sealed class GameApiClient(HttpClient httpClient, JsonSerializerOptions? jsonSerializerOptions = null)
{
    private const string BaseUrl = "http://localhost:5151/api/game";
    private readonly JsonSerializerOptions jsonOptions = jsonSerializerOptions ?? new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<StartGameResponse> StartAsync(StartGameRequest request)
    {
        var response = await httpClient.PostAsJsonAsync($"{BaseUrl}/start", request, jsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<StartGameResponse>(jsonOptions))
            ?? throw new InvalidOperationException("The Game API returned an empty start response.");
    }

    public async Task<GuessResponse> GuessAsync(GuessRequest request)
    {
        var response = await httpClient.PostAsJsonAsync($"{BaseUrl}/guess", request, jsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<GuessResponse>(jsonOptions))
            ?? throw new InvalidOperationException("The Game API returned an empty guess response.");
    }

    public async Task<HintResponse> HintAsync(string gameId)
    {
        var response = await httpClient.PostAsJsonAsync($"{BaseUrl}/hint", new GameIdRequest(gameId), jsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<HintResponse>(jsonOptions))
            ?? throw new InvalidOperationException("The Game API returned an empty hint response.");
    }

    public async Task<StartGameResponse> NextRoundAsync(string gameId)
    {
        var response = await httpClient.PostAsync($"{BaseUrl}/next-round?gameId={Uri.EscapeDataString(gameId)}", null);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<StartGameResponse>(jsonOptions))
            ?? throw new InvalidOperationException("The Game API returned an empty next-round response.");
    }

    public async Task StopAsync(string gameId)
    {
        var response = await httpClient.PostAsJsonAsync($"{BaseUrl}/stop", new GameIdRequest(gameId), jsonOptions);
        response.EnsureSuccessStatusCode();
    }
}

using System.Net.Http;
using System.Net.Http.Json;
using Desktop.Game.Models;

namespace Desktop.Game.Api;

public sealed class SettingsApiClient(HttpClient httpClient)
{
    private const string BaseUrl = "http://localhost:5151/api/settings";

    public async Task<PlayerSettingsResponse> GetAsync(string playerName)
    {
        var response = await httpClient.GetAsync($"{BaseUrl}/{Uri.EscapeDataString(playerName)}");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PlayerSettingsResponse>())
            ?? throw new InvalidOperationException("The Game API returned an empty settings response.");
    }

    public async Task<PlayerSettingsResponse> SaveAsync(string playerName, SavePlayerSettingsRequest request)
    {
        var response = await httpClient.PutAsJsonAsync($"{BaseUrl}/{Uri.EscapeDataString(playerName)}", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PlayerSettingsResponse>())
            ?? throw new InvalidOperationException("The Game API returned an empty settings response.");
    }
}

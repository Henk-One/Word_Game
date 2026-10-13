using System.Net;
using System.Net.Http.Json;

namespace Game.Api.E2EApiTests;

public class GameSessionE2ETests : IClassFixture<E2ETestContext>
{
    private readonly E2ETestContext _context;

    public GameSessionE2ETests(E2ETestContext context)
    {
        _context = context;
    }

  [Fact]
public async Task Journey1_PlayerGuessesIncorrectly_TriggersGameOverState()
{
    var client = _context.Client;

    // 1. Initial Session Provisioning
    var startRequest = new TestStartRequest("english", "beginner", "car-parts", "E2ELOSE");
    var startMsg = await client.PostAsJsonAsync("start", startRequest);
    Assert.Equal(HttpStatusCode.OK, startMsg.StatusCode);

    var game = await startMsg.Content.ReadFromJsonAsync<TestStartResponse>();
    Assert.NotNull(game);

    // 2. DYNAMICALLY build a list of letters GUARANTEED to be incorrect
    var wordLetters = game.FullWord.ToUpperInvariant();
    var badGuesses = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
        .Select(c => c.ToString())
        .Where(letter => !wordLetters.Contains(letter))
        .Take(11) // Take exactly enough to drain the 11 lives
        .ToArray();

    TestGuessResponse? lastGuessResult = null;

    foreach (var letter in badGuesses)
    {
        if (lastGuessResult?.GameOver == true) break;

        var res = await client.PostAsJsonAsync("guess", new TestGuessRequest(game.GameId, letter));
        lastGuessResult = await res.Content.ReadFromJsonAsync<TestGuessResponse>();
    }

    // 3. Confirm target terminal conditions match structural assertions
    Assert.NotNull(lastGuessResult);
    Assert.True(lastGuessResult.GameOver);
    Assert.Equal(0, lastGuessResult.Lives);
}


[Fact]
public async Task Journey2_CompleteSuccessfulGameWord_SubmitsToLeaderboard_AndCleansUp()
{
    var client = _context.Client;
    
    var targetPlayer = $"E2E{Guid.NewGuid():N}"[..8];

    try
    {
        // 1. Establish core target match boundaries
        var startRequest = new TestStartRequest("english", "beginner", "car-parts", targetPlayer);
        var startMsg = await client.PostAsJsonAsync("start", startRequest);
        var game = await startMsg.Content.ReadFromJsonAsync<TestStartResponse>();
        Assert.NotNull(game);

        // 2. Drive standard user interface responses
        var lettersToGuess = game.FullWord.Distinct().Select(c => c.ToString()).ToList();
        TestGuessResponse? finalGuessResponse = null;

        foreach (var character in lettersToGuess)
        {
            var res = await client.PostAsJsonAsync("guess", new TestGuessRequest(game.GameId, character));
            finalGuessResponse = await res.Content.ReadFromJsonAsync<TestGuessResponse>();
        }

        Assert.NotNull(finalGuessResponse);
        Assert.True(finalGuessResponse.RoundComplete);

        // 3. Manually trigger standard session closure routine
        var finishMsg = await client.PostAsJsonAsync("finish", new TestFinishRequest(game.GameId));
        Assert.Equal(HttpStatusCode.OK, finishMsg.StatusCode);

        var finalSummary = await finishMsg.Content.ReadFromJsonAsync<TestFinishResponse>();
        Assert.NotNull(finalSummary);
        Assert.True(finalSummary.QualifiesForLeaderboard);
    }
    finally
    {
        // 4. CLEANUP CYCLE: Fire an external HTTP call directly to the administration endpoint
        using var cleanupClient = new HttpClient();
        var leaderboardAdminUrl = $"http://localhost:5000/api/admin/purge-player?playerName={Uri.EscapeDataString(targetPlayer)}";
        
        var cleanupResponse = await cleanupClient.DeleteAsync(leaderboardAdminUrl);
        Assert.True(
            cleanupResponse.IsSuccessStatusCode || cleanupResponse.StatusCode == HttpStatusCode.NotFound,
            $"Failed to clean up test player '{targetPlayer}': {cleanupResponse.StatusCode}");
    }
}

}

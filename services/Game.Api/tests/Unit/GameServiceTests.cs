using Game.Api.Data;
using Game.Api.Models;
using Game.Api.Services;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Xunit;

namespace Game.UnitTests;

public class GameServiceTests
{
    private GameDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new GameDbContext(options);
    }

    private IWordProvider CreateWordProvider(string word)
    {
        return new TestWordProvider(word);
    }

    private GameService CreateService(GameDbContext db, IWordProvider provider)
    {
        var httpClient = new HttpClient(new SuccessfulHttpMessageHandler())
        {
            BaseAddress = new Uri("http://localhost")
        };

        return new GameService(db, provider, new LeaderboardClient(httpClient));
    }

    // -------------------------------------------------------------------------
    // RULE 1: Initial Session Creation
    // -------------------------------------------------------------------------
    [Fact]
    public async Task StartGame_ShouldCreateSession_WithCorrectInitialValues()
    {
        var db = CreateDbContext();
        var provider = CreateWordProvider("TESTWORD");
        var service = CreateService(db, provider);

        var req = new StartGameRequest("english", "beginner", "car-parts", "Player1");
        var result = await service.StartAsync(req);

        var session = await db.GameSessions.FindAsync(result.GameId);

        Assert.NotNull(session);
        Assert.Equal("TESTWORD", session.Word);
        Assert.Equal("--------", session.Revealed);
        Assert.Equal(11, session.Lives);
        Assert.Equal(0, session.Score);
        Assert.False(session.GameOver);
    }

    // -------------------------------------------------------------------------
    // RULE 2: Correct Letter Guess (Score shouldn't increase per character)
    // -------------------------------------------------------------------------
    [Fact]
    public async Task Guess_ShouldRevealLetters_ButNotIncreaseScore_PerIndividualCharacter()
    {
        var db = CreateDbContext();
        var provider = CreateWordProvider("CAR");
        var service = CreateService(db, provider);

        var start = await service.StartAsync(new StartGameRequest("english", "beginner", "car-parts", "Player1"));

        var guess = await service.GuessAsync(new GuessRequest(start.GameId, "C"));

        Assert.Equal("C--", guess.Revealed);
        Assert.Equal(0, guess.Score); // Score remains 0; it only increases when the whole word is completed
        Assert.Equal(11, guess.Lives);
        Assert.False(guess.GameOver);
    }

    // -------------------------------------------------------------------------
    // RULE 3: Incorrect Letter Guess (Reduces a heart)
    // -------------------------------------------------------------------------
    [Fact]
    public async Task Guess_ShouldReduceLives_WhenLetterIsIncorrect()
    {
        var db = CreateDbContext();
        var provider = CreateWordProvider("CAR");
        var service = CreateService(db, provider);

        var start = await service.StartAsync(new StartGameRequest("english", "beginner", "car-parts", "Player1"));

        var guess = await service.GuessAsync(new GuessRequest(start.GameId, "Z"));

        Assert.Equal("---", guess.Revealed);
        Assert.Equal(0, guess.Score);
        Assert.Equal(10, guess.Lives); // Lost 1 heart
        Assert.False(guess.GameOver);
    }

    // -------------------------------------------------------------------------
    // RULE 4: Word Completed (Score increases now, game continues)
    // -------------------------------------------------------------------------
    [Fact]
    public async Task Guess_ShouldIncreaseScore_OnlyWhenTheEntireWordIsGuessed()
    {
        var db = CreateDbContext();
        var provider = CreateWordProvider("HI");
        var service = CreateService(db, provider);

        var start = await service.StartAsync(new StartGameRequest("english", "beginner", "car-parts", "Player1"));

        await service.GuessAsync(new GuessRequest(start.GameId, "H"));
        var final = await service.GuessAsync(new GuessRequest(start.GameId, "I"));

        Assert.True(final.RoundComplete);
        Assert.Equal(1, final.Score); // Score increases because the full word is guessed
        Assert.Equal(1, final.SolvedWords);
        Assert.False(final.GameOver); // Game is still active (moves to next round/word)
    }

    // -------------------------------------------------------------------------
    // RULE 5: Game Over (Happens only when running out of hearts)
    // -------------------------------------------------------------------------
    [Fact]
    public async Task Guess_ShouldTriggerGameOver_WhenPlayerRunsOutOfHearts()
    {
        var db = CreateDbContext();
        var provider = CreateWordProvider("A");
        var service = CreateService(db, provider);

        var start = await service.StartAsync(new StartGameRequest("english", "beginner", "car-parts", "Player1"));

        // Level 1 has no repeat penalty, so we must use unique incorrect letters to drain 11 lives
        string[] uniqueIncorrectLetters = { "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L" };

        for (int i = 0; i < 11; i++)
        {
            await service.GuessAsync(new GuessRequest(start.GameId, uniqueIncorrectLetters[i]));
        }

        var session = await db.GameSessions.FindAsync(start.GameId);

        Assert.NotNull(session);
        Assert.True(session.GameOver); // True game over because hearts hit zero
        Assert.Equal(0, session.Lives);
    }

    // -------------------------------------------------------------------------
    // RULE 6: Finish Score Submission
    // -------------------------------------------------------------------------
    [Fact]
    public async Task Finish_ShouldReturnAccumulatedScore_FromCompletedWords()
    {
        var db = CreateDbContext();
        var provider = CreateWordProvider("A");
        var service = CreateService(db, provider);

        var start = await service.StartAsync(new StartGameRequest("english", "beginner", "car-parts", "Player1"));

        await service.GuessAsync(new GuessRequest(start.GameId, "A")); // Guessed word successfully, score = 1

        var finish = await service.FinishAsync(new FinishGameRequest(start.GameId));

        Assert.Equal(1, finish.FinalScore);
        Assert.True(finish.QualifiesForLeaderboard);
    }

    // -------------------------------------------------------------------------
    // NextRound clears guessed letters
    // -------------------------------------------------------------------------
    [Fact]
    public async Task NextRound_ShouldClearGuessedLettersSet()
    {
        var db = CreateDbContext();
        var provider = CreateWordProvider("CAT");
        var service = CreateService(db, provider);

        var start = await service.StartAsync(new StartGameRequest("english", "beginner", "car-parts", "Player1"));
        await service.GuessAsync(new GuessRequest(start.GameId, "C"));
        await service.GuessAsync(new GuessRequest(start.GameId, "X"));

        var sessionBefore = await db.GameSessions.FindAsync(start.GameId);
        Assert.NotEmpty(sessionBefore!.GetGuessedSet());

        await service.NextRoundAsync(start.GameId);

        var sessionAfter = await db.GameSessions.FindAsync(start.GameId);
        Assert.NotNull(sessionAfter);
        Assert.Empty(sessionAfter.GetGuessedSet());
        Assert.Equal("", sessionAfter.GuessedLetters);
    }

    // -----------------------------
    // Helper provider
    // -----------------------------
    private class TestWordProvider : IWordProvider
    {
        private readonly string _word;

        public TestWordProvider(string word)
        {
            _word = word.ToUpperInvariant();
        }

        public Task<string> GetRandomWordAsync(string language, string difficulty, string topic)
        {
            return Task.FromResult(_word);
        }
    }

    private sealed class SuccessfulHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"qualifies":true}""",
                    System.Text.Encoding.UTF8,
                    "application/json")
            });
        }
    }
}

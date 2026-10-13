using Game.Api.Data;
using Game.Api.Models;

namespace Game.Api.Services;

public class GameService : IGameService
{
    private readonly GameDbContext _db;
    private readonly IWordProvider _wordProvider;
    private readonly LeaderboardClient _leaderboardClient;

    private const int MaxLives = 11;
    private const int MaxLevel = 10;
    private const int WordsPerLevelDefault = 3;
    private const int MaxHints = 5;

    public GameService(GameDbContext db, IWordProvider wordProvider, LeaderboardClient leaderboardClient)
    {
        _db = db;
        _wordProvider = wordProvider;
        _leaderboardClient = leaderboardClient;
    }

    public async Task<StartGameResponse> StartAsync(StartGameRequest request)
    {
        var word = await _wordProvider.GetRandomWordAsync(
            request.Language,
            request.Difficulty,
            request.Topic);

        if (string.IsNullOrWhiteSpace(word))
            throw new InvalidOperationException(
                "No word available for selection.");

        word = word.Trim().ToUpperInvariant();

        var level = 1;
        var livesPerLevel = ComputeLivesForLevel(level);
        var hintCapacity = ComputeHintCapacityForLevel(level);
        var repeatPenalty = ComputeRepeatPenaltyForLevel(level);

        var session = new GameSession
        {
            PlayerName = request.PlayerName ?? "",
            Language = request.Language,
            Difficulty = request.Difficulty,
            Topic = request.Topic,
            Word = word,
            Revealed = new string('-', word.Length),
            Lives = livesPerLevel,
            Score = 0,
            GameOver = false,
            GuessedLetters = "",
            Level = level,
            SolvedWords = 0,
            WordsPerLevel = WordsPerLevelDefault,
            MaxLevel = MaxLevel,
            Hints = 0,
            HintCapacity = hintCapacity,
            BonusHintProgress = 0,
            RoundNumber = 1,
            RoundActive = true,
            RepeatGuessPenalty = repeatPenalty,
            LivesPerLevel = livesPerLevel
        };

        _db.GameSessions.Add(session);
        await _db.SaveChangesAsync();

        return new StartGameResponse(
            GameId: session.Id,
            WordLength: session.Word.Length,
            Revealed: session.Revealed,
            FullWord: session.Word,
            Lives: session.Lives,
            Score: session.Score,
            Level: session.Level,
            Hints: session.Hints,
            HintCapacity: session.HintCapacity,
            BonusHintProgress: session.BonusHintProgress,
            SolvedWords: session.SolvedWords,
            RoundNumber: session.RoundNumber,
            RepeatGuessPenalty: session.RepeatGuessPenalty,
            LivesPerLevel: session.LivesPerLevel
        );
    }

    public async Task<StartGameResponse> NextRoundAsync(string gameId)
    {
        var session = await _db.GameSessions.FindAsync(gameId);

        if (session is null)
            throw new KeyNotFoundException("Game session not found.");

        var word = await _wordProvider.GetRandomWordAsync(
            session.Language,
            session.Difficulty,
            session.Topic);

        if (string.IsNullOrWhiteSpace(word))
            throw new InvalidOperationException(
                "No word available for selection.");

        word = word.Trim().ToUpperInvariant();

        session.Word = word;
        session.Revealed = new string('-', word.Length);
        session.GuessedLetters = "";
        session.RoundActive = true;

        // Restore lives for the current level
        session.Lives = session.LivesPerLevel;
        Console.WriteLine($"NextRound: lives={session.Lives}, livesPerLevel={session.LivesPerLevel}");

        await _db.SaveChangesAsync();

        return new StartGameResponse(
            GameId: session.Id,
            WordLength: session.Word.Length,
            Revealed: session.Revealed,
            FullWord: session.Word,
            Lives: session.Lives,
            Score: session.Score,
            Level: session.Level,
            Hints: session.Hints,
            HintCapacity: session.HintCapacity,
            BonusHintProgress: session.BonusHintProgress,
            SolvedWords: session.SolvedWords,
            RoundNumber: session.RoundNumber,
            RepeatGuessPenalty: session.RepeatGuessPenalty,
            LivesPerLevel: session.LivesPerLevel
        );
    }


    public async Task<GuessResponse> GuessAsync(GuessRequest request)
    {
        var session = await _db.GameSessions.FindAsync(request.GameId);

        if (session is null)
            throw new KeyNotFoundException("Game session not found.");

        if (session.GameOver || !session.RoundActive || session.Lives <= 0)
        {
            return BuildGuessResponse(session, roundComplete: false);
        }

        var letter = NormalizeLetter(request.Letter);
        var guessed = session.GetGuessedSet();

        if (guessed.Contains(letter))
        {
            if (session.RepeatGuessPenalty)
            {
                session.Lives = Math.Max(0, session.Lives - 1);
            }
        }
        else
        {
            guessed.Add(letter);
            session.SetGuessedSet(guessed);

            if (session.Word.Contains(letter))
            {
                session.Revealed =
                    RevealLetters(
                        session.Word,
                        session.Revealed,
                        letter);
            }
            else
            {
                session.Lives = Math.Max(0, session.Lives - 1);
            }
        }

        var roundComplete = !session.Revealed.Contains('-');

        string? completedReveal = null;
        string? completedWord = null;

        if (roundComplete)
        {
            completedReveal = session.Revealed;
            completedWord = session.Word;

            session.Score += 1;
            session.SolvedWords += 1;

            RewardHintForSolvedWord(session);
            AdvanceLevelIfNeeded(session);
        }

        if (session.Lives <= 0)
        {
            session.GameOver = true;
            session.RoundActive = false;
        }

        await _db.SaveChangesAsync();

        var response = BuildGuessResponse(session, roundComplete);

        if (roundComplete)
        {
            response = response with
            {
                Revealed = completedReveal!,
                FullWord = completedWord!
            };
        }

        return response;
    }

    public async Task<HintResponse> HintAsync(HintRequest request)
    {
        var session = await _db.GameSessions.FindAsync(request.GameId);

        if (session is null)
            throw new KeyNotFoundException("Game session not found.");

        if (session.GameOver ||
            !session.RoundActive ||
            session.Lives <= 0 ||
            session.Hints <= 0)
        {
            return new HintResponse(
                GameId: session.Id,
                Revealed: session.Revealed,
                FullWord: session.Word,
                Lives: session.Lives,
                Score: session.Score,
                Level: session.Level,
                Hints: session.Hints,
                HintCapacity: session.HintCapacity,
                BonusHintProgress: session.BonusHintProgress,
                SolvedWords: session.SolvedWords,
                RoundNumber: session.RoundNumber,
                RoundComplete: false
            );
        }

        var index = session.Revealed.IndexOf('-');

        if (index == -1)
        {
            return new HintResponse(
                GameId: session.Id,
                Revealed: session.Revealed,
                FullWord: session.Word,
                Lives: session.Lives,
                Score: session.Score,
                Level: session.Level,
                Hints: session.Hints,
                HintCapacity: session.HintCapacity,
                BonusHintProgress: session.BonusHintProgress,
                SolvedWords: session.SolvedWords,
                RoundNumber: session.RoundNumber,
                RoundComplete: false
            );
        }

        var letter = session.Word[index];

        var guessed = session.GetGuessedSet();
        guessed.Add(letter);
        session.SetGuessedSet(guessed);

        var chars = session.Revealed.ToCharArray();
        chars[index] = letter;
        session.Revealed = new string(chars);

        session.Hints = Math.Max(0, session.Hints - 1);

        var roundComplete = !session.Revealed.Contains('-');

        string? completedReveal = null;
        string? completedWord = null;

        if (roundComplete)
        {
            completedReveal = session.Revealed;
            completedWord = session.Word;

            session.Score += 1;
            session.SolvedWords += 1;

            RewardHintForSolvedWord(session);
            AdvanceLevelIfNeeded(session);
        }

        await _db.SaveChangesAsync();

        var response = new HintResponse(
            GameId: session.Id,
            Revealed: session.Revealed,
            FullWord: session.Word,
            Lives: session.Lives,
            Score: session.Score,
            Level: session.Level,
            Hints: session.Hints,
            HintCapacity: session.HintCapacity,
            BonusHintProgress: session.BonusHintProgress,
            SolvedWords: session.SolvedWords,
            RoundNumber: session.RoundNumber,
            RoundComplete: roundComplete
        );

        if (roundComplete)
        {
            response = response with
            {
                Revealed = completedReveal!,
                FullWord = completedWord!
            };
        }

        return response;
    }



    public async Task<FinishGameResponse> FinishAsync(FinishGameRequest request)
    {
        var session = await _db.GameSessions.FindAsync(request.GameId);
        if (session is null)
            throw new KeyNotFoundException("Game session not found.");

        if (!session.GameOver)
        {
            session.GameOver = true;
            await _db.SaveChangesAsync();
        }


        var finalScore = session.Score;

        var qualifies = await _leaderboardClient.SubmitScoreAsync(
            session.PlayerName,
            finalScore,
            session.Language,
            session.Difficulty,
            session.Topic
        );

        return new FinishGameResponse(
            GameId: session.Id,
            FullWord: session.Word,
            FinalScore: finalScore,
            QualifiesForLeaderboard: qualifies,
            Level: session.Level,
            SolvedWords: session.SolvedWords
        );
    }

    private static char NormalizeLetter(string letter)
    {
        if (string.IsNullOrWhiteSpace(letter) || letter.Length != 1)
            throw new ArgumentException("Letter must be a single character.", nameof(letter));

        var c = char.ToUpperInvariant(letter[0]);
        if (c < 'A' || c > 'Z')
            throw new ArgumentException("Letter must be A-Z.", nameof(letter));

        return c;
    }

    private static string RevealLetters(string word, string revealed, char letter)
    {
        var chars = revealed.ToCharArray();
        for (int i = 0; i < word.Length; i++)
        {
            if (word[i] == letter)
            {
                chars[i] = letter;
            }
        }
        return new string(chars);
    }

    private static int ComputeLivesForLevel(int level)
    {
        if (level <= 2) return MaxLives;
        return Math.Max(8, MaxLives - (level - 2));
    }

    private static int ComputeHintCapacityForLevel(int level)
    {
        return Math.Max(1, MaxHints - Math.Max(0, level - 6));
    }

    private static bool ComputeRepeatPenaltyForLevel(int level)
    {
        return level > 1;
    }

    private static void RewardHintForSolvedWord(GameSession session)
    {
        if (session.Level < 6)
        {
            if (session.Hints < session.HintCapacity)
            {
                session.Hints++;
            }
        }
        else
        {
            if (session.BonusHintProgress >= 2)
            {
                session.BonusHintProgress = 0;
            }
            session.BonusHintProgress++;

            if (session.BonusHintProgress == 2)
            {
                if (session.Hints < session.HintCapacity)
                {
                    session.Hints++;
                }
            }
        }
    }

    private static void AdvanceLevelIfNeeded(GameSession session)
    {
        var nextLevel = Math.Min(
            session.MaxLevel,
            1 + session.SolvedWords / session.WordsPerLevel
        );

        if (nextLevel > session.Level)
        {
            session.Level = nextLevel;
            session.LivesPerLevel = ComputeLivesForLevel(session.Level);
            session.Lives = session.LivesPerLevel;
            session.HintCapacity = ComputeHintCapacityForLevel(session.Level);
            if (session.Hints > session.HintCapacity)
            {
                session.Hints = session.HintCapacity;
            }
            session.RepeatGuessPenalty = ComputeRepeatPenaltyForLevel(session.Level);
            session.RoundNumber += 1;
            session.RoundActive = true;
            session.Revealed = new string('-', session.Word.Length);
            session.GuessedLetters = "";
        }
        else
        {
            session.RoundNumber += 1;
            session.RoundActive = true;
            session.Revealed = new string('-', session.Word.Length);
            session.GuessedLetters = "";
        }
    }

    private static GuessResponse BuildGuessResponse(GameSession session, bool roundComplete)
    {
        return new GuessResponse(
            GameId: session.Id,
            Revealed: session.Revealed,
            FullWord: session.Word,
            Lives: session.Lives,
            Score: session.Score,
            GameOver: session.GameOver,
            Level: session.Level,
            Hints: session.Hints,
            HintCapacity: session.HintCapacity,
            BonusHintProgress: session.BonusHintProgress,
            SolvedWords: session.SolvedWords,
            RoundNumber: session.RoundNumber,
            RoundComplete: roundComplete,
            RepeatGuessPenalty: session.RepeatGuessPenalty,
            LivesPerLevel: session.LivesPerLevel
        );
    }
}

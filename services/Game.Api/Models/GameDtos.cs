namespace Game.Api.Models;

public record StartGameRequest(
    string Language,
    string Difficulty,
    string Topic,
    string PlayerName
);

public record StartGameResponse(
    string GameId,
    int WordLength,
    string Revealed,
    string FullWord,
    int Lives,
    int Score,
    int Level,
    int Hints,
    int HintCapacity,
    int BonusHintProgress,
    int SolvedWords,
    int RoundNumber,
    bool RepeatGuessPenalty,
    int LivesPerLevel
);

public record GuessRequest(
    string GameId,
    string Letter
);

public record GuessResponse(
    string GameId,
    string Revealed,
    string FullWord,
    int Lives,
    int Score,
    bool GameOver,
    int Level,
    int Hints,
    int HintCapacity,
    int BonusHintProgress,
    int SolvedWords,
    int RoundNumber,
    bool RoundComplete,
    bool RepeatGuessPenalty,
    int LivesPerLevel
);

public record HintRequest(
    string GameId
);

public record HintResponse(
    string GameId,
    string Revealed,
    string FullWord,
    int Lives,
    int Score,
    int Level,
    int Hints,
    int HintCapacity,
    int BonusHintProgress,
    int SolvedWords,
    int RoundNumber,
    bool RoundComplete
);

public record FinishGameRequest(
    string GameId
);

public record FinishGameResponse(
    string GameId,
    string FullWord,
    int FinalScore,
    bool QualifiesForLeaderboard,
    int Level,
    int SolvedWords
);

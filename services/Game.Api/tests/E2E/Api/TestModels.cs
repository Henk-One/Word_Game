namespace Game.Api.E2EApiTests;

public record TestStartRequest(string Language, string Difficulty, string Topic, string PlayerName);

public record TestStartResponse(
    string GameId, 
    int WordLength, 
    string Revealed, 
    string FullWord, 
    int Lives, 
    int Score, 
    int Level
);

public record TestGuessRequest(string GameId, string Letter);

public record TestGuessResponse(
    string GameId, 
    string Revealed, 
    string FullWord, 
    int Lives, 
    int Score, 
    bool GameOver, 
    int Level, 
    bool RoundComplete
);

public record TestFinishRequest(string GameId);

public record TestFinishResponse(
    string GameId, 
    string FullWord, 
    int FinalScore, 
    bool QualifiesForLeaderboard
);

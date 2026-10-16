namespace Desktop.Game.Models;

public sealed record StartGameRequest(string Language, string Difficulty, string Topic, string PlayerName);
public sealed record GameIdRequest(string GameId);
public sealed record GuessRequest(string GameId, string Letter);

public sealed record StartGameResponse(string GameId, int WordLength, string Revealed, string FullWord, int Lives, int Score, int Level, int Hints, int HintCapacity, int BonusHintProgress, int SolvedWords, int RoundNumber, bool RepeatGuessPenalty, int LivesPerLevel);
public sealed record GuessResponse(string GameId, string Revealed, string FullWord, int Lives, int Score, bool GameOver, int Level, int Hints, int HintCapacity, int BonusHintProgress, int SolvedWords, int RoundNumber, bool RoundComplete, bool RepeatGuessPenalty, int LivesPerLevel);
public sealed record HintResponse(string GameId, string Revealed, string FullWord, int Lives, int Score, int Level, int Hints, int HintCapacity, int BonusHintProgress, int SolvedWords, int RoundNumber, bool RoundComplete);

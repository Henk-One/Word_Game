namespace Desktop.Game.Models;

public sealed record HighScoreResponse(string PlayerName, int Score, DateTime Date, string? Error = null);

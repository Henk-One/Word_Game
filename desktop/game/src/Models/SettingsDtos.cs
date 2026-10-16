namespace Desktop.Game.Models;

public sealed record PlayerSettingsResponse(string PlayerName, string Language, string Difficulty, string Topic);
public sealed record SavePlayerSettingsRequest(string Language, string Difficulty, string Topic);

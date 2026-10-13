namespace Leaderboard.Api.Models;

public class SubmitScoreRequest
{
    public string PlayerName { get; set; } = "";
    public int Score { get; set; }
    public string Language { get; set; } = "";
    public string Difficulty { get; set; } = "";
    public string Topic { get; set; } = "";
}

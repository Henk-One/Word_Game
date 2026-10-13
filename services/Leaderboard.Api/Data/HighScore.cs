using System.ComponentModel.DataAnnotations;

namespace Leaderboard.Api.Data;

public class HighScore
{
    [Key]
    public int Id { get; set; }

    [MaxLength(8)]
    public string PlayerName { get; set; } = "";

    public int Score { get; set; }

    public string Language { get; set; } = "";
    public string Difficulty { get; set; } = "";
    public string Topic { get; set; } = "";

    public DateTime Date { get; set; } = DateTime.UtcNow;
}

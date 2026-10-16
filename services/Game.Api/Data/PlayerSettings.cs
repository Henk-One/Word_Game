using System.ComponentModel.DataAnnotations;

namespace Game.Api.Data;

public class PlayerSettings
{
    [Key]
    [MaxLength(8)]
    public string PlayerName { get; set; } = "";

    [MaxLength(20)]
    public string Language { get; set; } = "english";

    [MaxLength(20)]
    public string Difficulty { get; set; } = "beginner";

    [MaxLength(40)]
    public string Topic { get; set; } = "car-parts";
}

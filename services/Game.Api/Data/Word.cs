namespace Game.Api.Data;

public class Word
{
    public int Id { get; set; }
    public string Language { get; set; } = default!;
    public string Difficulty { get; set; } = default!;
    public string Topic { get; set; } = default!; // Added column for categorical filtering
    public string Text { get; set; } = default!;
}

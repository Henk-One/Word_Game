using System.ComponentModel.DataAnnotations;

namespace Game.Api.Data;

public class GameSession
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [MaxLength(8)]
    public string PlayerName { get; set; } = "";

    public string Language { get; set; } = "";
    public string Difficulty { get; set; } = "";
    public string Topic { get; set; } = "";

    // Core Word Guess state
    public string Word { get; set; } = "";
    public string Revealed { get; set; } = "";
    public string GuessedLetters { get; set; } = "";
    public int Lives { get; set; }
    public int Score { get; set; }
    public bool GameOver { get; set; }

    // Leveling
    public int Level { get; set; } = 1;
    public int SolvedWords { get; set; } = 0;
    public int WordsPerLevel { get; set; } = 3;   // A: 3 words per level
    public int MaxLevel { get; set; } = 10;

    // Hints
    public int Hints { get; set; } = 0;
    public int HintCapacity { get; set; } = 3;
    public int BonusHintProgress { get; set; } = 0;

    // Round state
    public int RoundNumber { get; set; } = 1;
    public bool RoundActive { get; set; } = true;

    // Rules
    public bool RepeatGuessPenalty { get; set; } = false;
    public int LivesPerLevel { get; set; } = 11;

    // Helpers for guessed letters
    public HashSet<char> GetGuessedSet()
    {
        var set = new HashSet<char>();
        if (!string.IsNullOrWhiteSpace(GuessedLetters))
        {
            foreach (var c in GuessedLetters)
            {
                set.Add(c);
            }
        }
        return set;
    }

    public void SetGuessedSet(HashSet<char> set)
    {
        GuessedLetters = new string(set.OrderBy(c => c).ToArray());
    }
}

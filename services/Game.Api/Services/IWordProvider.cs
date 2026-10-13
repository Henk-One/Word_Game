using Game.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Game.Api.Services;

public interface IWordProvider
{
    Task<string> GetRandomWordAsync(string language, string difficulty, string topic);
}

public class DbWordProvider : IWordProvider
{
    private readonly GameDbContext _context;

    public DbWordProvider(GameDbContext context)
    {
        _context = context;
    }

    public async Task<string> GetRandomWordAsync(string language, string difficulty, string topic)
    {
        var normalizedLanguage = language.Trim().ToLowerInvariant();
        var normalizedDifficulty = difficulty.Trim().ToLowerInvariant();
        var normalizedTopic = topic.Trim().ToLowerInvariant();

        // Get matching word texts across language, difficulty, and theme constraints
        var wordsList = await _context.Words
            .Where(w => w.Language == normalizedLanguage && 
                        w.Difficulty == normalizedDifficulty && 
                        w.Topic == normalizedTopic)
            .Select(w => w.Text)
            .ToListAsync();

        if (wordsList.Count == 0)
        {
            return string.Empty;
        }

        // Return a reliable pseudo-random structural element
        var index = Random.Shared.Next(wordsList.Count);
        return wordsList[index];
    }
}

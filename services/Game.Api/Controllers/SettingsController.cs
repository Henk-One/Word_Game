using Game.Api.Data;
using Game.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Game.Api.Controllers;

[ApiController]
[Route("api/settings")]
public class SettingsController : ControllerBase
{
    private readonly GameDbContext db;

    public SettingsController(GameDbContext db) => this.db = db;

    [HttpGet("{playerName}")]
    public async Task<ActionResult<PlayerSettingsResponse>> Get(string playerName)
    {
        var settings = await db.PlayerSettings.FindAsync(playerName.Trim());
        return Ok(settings is null
            ? new PlayerSettingsResponse(playerName.Trim(), "english", "beginner", "car-parts")
            : new PlayerSettingsResponse(settings.PlayerName, settings.Language, settings.Difficulty, settings.Topic));
    }

    [HttpPut("{playerName}")]
    public async Task<ActionResult<PlayerSettingsResponse>> Save(string playerName, SavePlayerSettingsRequest request)
    {
        var normalizedName = playerName.Trim();
        if (string.IsNullOrWhiteSpace(normalizedName) || normalizedName.Length > 8)
            return BadRequest("Player name must be between 1 and 8 characters.");

        var settings = await db.PlayerSettings.FindAsync(normalizedName) ?? new PlayerSettings { PlayerName = normalizedName };
        settings.Language = request.Language.Trim().ToLowerInvariant();
        settings.Difficulty = request.Difficulty.Trim().ToLowerInvariant();
        settings.Topic = request.Topic.Trim().ToLowerInvariant();
        if (db.Entry(settings).State == EntityState.Detached) db.PlayerSettings.Add(settings);
        await db.SaveChangesAsync();
        return Ok(new PlayerSettingsResponse(settings.PlayerName, settings.Language, settings.Difficulty, settings.Topic));
    }
}

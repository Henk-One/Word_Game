using Leaderboard.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Leaderboard.Api.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly ILeaderboardService _service;

    public AdminController(ILeaderboardService service)
    {
        _service = service;
    }

    [HttpDelete("purge-player")]
    [EndpointSummary("Purge a player's high scores")]
    [EndpointDescription("Administrative endpoint to cleanly remove a test player from database boundaries.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PurgePlayer([FromQuery] string playerName)
    {
        var success = await _service.RemovePlayerScoresAsync(playerName);
        if (!success) return NotFound($"No highscores found for player '{playerName}'.");
        return Ok(new { Message = $"Successfully removed all scores for {playerName}." });
    }
}

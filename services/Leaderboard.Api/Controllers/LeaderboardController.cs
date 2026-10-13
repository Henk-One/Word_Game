using Leaderboard.Api.Data;
using Leaderboard.Api.Models;
using Leaderboard.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Leaderboard.Api.Controllers;

[ApiController]
[Route("api/leaderboard")]
public class LeaderboardController : ControllerBase
{
    private readonly ILeaderboardService _service;

    public LeaderboardController(ILeaderboardService service)
    {
        _service = service;
    }

    [HttpGet("{language}/{difficulty}/{topic}")]
    [EndpointSummary("Get the top scores")]
    [EndpointDescription("Returns up to ten scores for a language, difficulty, and topic.")]
    [ProducesResponseType(typeof(List<HighScore>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<HighScore>>> GetTopScores(
        string language, string difficulty, string topic)
    {
        var scores = await _service.GetTopScoresAsync(language, difficulty, topic);
        return Ok(scores);
    }

    [HttpPost("submit")]
    [EndpointSummary("Submit a score")]
    [EndpointDescription("Submits a score and reports whether it is retained in the category's top ten.")]
    [ProducesResponseType(typeof(SubmitScoreResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SubmitScoreResponse>> SubmitScore([FromBody] SubmitScoreRequest request)
    {
        var qualifies = await _service.SubmitScoreAsync(request);
        return Ok(new SubmitScoreResponse(qualifies));
    }
}

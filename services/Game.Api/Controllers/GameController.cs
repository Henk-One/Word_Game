using Game.Api.Models;
using Game.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Game.Api.Controllers;

[ApiController]
[Route("api/game")]
public class GameController : ControllerBase
{
    private readonly IGameService _service;

    public GameController(IGameService service)
    {
        _service = service;
    }

    // ---------------------------------------------------------
    // START GAME (first round)
    // ---------------------------------------------------------
    [HttpPost("start")]
    [EndpointSummary("Start a game")]
    [EndpointDescription("Starts a game using the requested language, difficulty, topic, and player name.")]
    [ProducesResponseType(typeof(StartGameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StartGameResponse>> Start([FromBody] StartGameRequest request)
    {
        if (request is null)
            return BadRequest("Invalid request.");

        var result = await _service.StartAsync(request);
        return Ok(result);
    }

    [HttpPost("next-round")]
    [EndpointSummary("Start the next round")]
    [EndpointDescription("Starts the next round for the game identified by the gameId query parameter.")]
    [ProducesResponseType(typeof(StartGameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StartGameResponse>> NextRound([FromQuery] string gameId)
    {
        var response = await _service.NextRoundAsync(gameId);
        return Ok(response);
    }

    // ---------------------------------------------------------
    // GUESS LETTER
    // ---------------------------------------------------------
    [HttpPost("guess")]
    [EndpointSummary("Submit a letter guess")]
    [EndpointDescription("Submits a letter guess for an active game.")]
    [ProducesResponseType(typeof(GuessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GuessResponse>> Guess([FromBody] GuessRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.GameId))
            return BadRequest("Invalid request.");

        var result = await _service.GuessAsync(request);
        return Ok(result);
    }

    // ---------------------------------------------------------
    // USE HINT
    // ---------------------------------------------------------
    [HttpPost("hint")]
    [EndpointSummary("Use a hint")]
    [EndpointDescription("Uses a hint for an active game.")]
    [ProducesResponseType(typeof(HintResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<HintResponse>> Hint([FromBody] HintRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.GameId))
            return BadRequest("Invalid request.");

        var result = await _service.HintAsync(request);
        return Ok(result);
    }

    // ---------------------------------------------------------
    // FINISH GAME
    // ---------------------------------------------------------
    [HttpPost("finish")]
    [EndpointSummary("Finish a game")]
    [EndpointDescription("Finishes a game and submits its server-calculated score to the leaderboard.")]
    [ProducesResponseType(typeof(FinishGameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FinishGameResponse>> Finish([FromBody] FinishGameRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.GameId))
            return BadRequest("Invalid request.");

        var result = await _service.FinishAsync(request);
        return Ok(result);
    }

    // ---------------------------------------------------------
    // QUIT GAME (Manual Stop)
    // ---------------------------------------------------------
    [HttpPost("stop")]
    [EndpointSummary("Stop a game")]
    [EndpointDescription("Manually stops a game and returns its final result.")]
    [ProducesResponseType(typeof(FinishGameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FinishGameResponse>> Stop([FromBody] FinishGameRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.GameId))
            return BadRequest("Invalid request.");

        // Manual stop dirctly ends the session
        var result = await _service.FinishAsync(new FinishGameRequest(request.GameId));
        return Ok(result);
    }

}

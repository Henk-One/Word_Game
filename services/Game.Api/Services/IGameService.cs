using Game.Api.Models;

namespace Game.Api.Services;

public interface IGameService
{
    Task<StartGameResponse> StartAsync(StartGameRequest request);
    Task<StartGameResponse> NextRoundAsync(string gameId);
    Task<GuessResponse> GuessAsync(GuessRequest request);
    Task<FinishGameResponse> FinishAsync(FinishGameRequest request);
    Task<HintResponse> HintAsync(HintRequest request);


}

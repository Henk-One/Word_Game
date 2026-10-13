namespace Game.Api.E2EApiTests;

public class E2ETestContext : IDisposable
{
    public HttpClient Client { get; }

    public E2ETestContext()
    {
        // Read configuration variable. Default to local project execution profile URL if empty
        var baseUrl = Environment.GetEnvironmentVariable("GAME_API_TEST_URL");
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            baseUrl = "http://localhost:5151/api/game";
        }

        Client = new HttpClient 
        { 
            BaseAddress = new Uri(baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/") 
        };
    }

    public void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }
}

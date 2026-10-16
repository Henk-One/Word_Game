namespace Game.Api.E2EApiTests;

public class E2ETestContext : IDisposable
{
    public HttpClient Client { get; }

    public E2ETestContext()
    {
        // Read configuration variable. Fail-fast if the target environment is not supplied.
        var baseUrl = Environment.GetEnvironmentVariable("GAME_API_TEST_URL");
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException(
                "The environment variable 'GAME_API_TEST_URL' is required but was not provided. " +
                "Please verify that the central pipeline configuration runner is active.");
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

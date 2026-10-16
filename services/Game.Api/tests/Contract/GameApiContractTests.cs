using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Game.Api.Data;
using Game.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Game.Api.ContractTests;

public class GameApiContractTests
{
    [Fact]
    public async Task StartGame_request_and_response_match_the_published_openapi_contract()
    {
        using var factory = new GameApiFactory();
        using var client = factory.CreateClient();

        var document = await client.GetFromJsonAsync<JsonObject>("/openapi/v1.json");
        Assert.NotNull(document);
        Assert.StartsWith("3.1.", document["openapi"]!.GetValue<string>());

        var operation = document["paths"]!["/api/game/start"]!["post"]!.AsObject();
        var requestSchema = ResolveSchema(
            document,
            operation["requestBody"]!["content"]!["application/json"]!["schema"]);
        Assert.Equal(
            new[] { "difficulty", "language", "playerName", "topic" },
            requestSchema["required"]!.AsArray()
                .Select(value => value!.GetValue<string>())
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray());

        using var response = await client.PostAsJsonAsync("/api/game/start", new
        {
            language = "english",
            difficulty = "beginner",
            topic = "car-parts",
            playerName = "Player"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseSchema = ResolveSchema(
            document,
            operation["responses"]!["200"]!["content"]!["application/json"]!["schema"]);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var actualProperties = body.RootElement.EnumerateObject()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        var contractProperties = responseSchema["properties"]!.AsObject().Select(property => property.Key)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(contractProperties, actualProperties);
        Assert.Equal("CAR", body.RootElement.GetProperty("fullWord").GetString());
        Assert.Equal("---", body.RootElement.GetProperty("revealed").GetString());
    }

    [Fact]
    public async Task NextRound_response_matches_the_published_openapi_contract()
    {
        using var factory = new GameApiFactory();
        using var client = factory.CreateClient();
        var document = await GetOpenApiDocument(client);
        var gameId = await StartGame(client);

        using var response = await client.PostAsync($"/api/game/next-round?gameId={gameId}", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await AssertResponsePropertiesMatchSchema(document, "/api/game/next-round", "post", "200", response);
    }

    [Fact]
    public async Task Guess_response_matches_the_published_openapi_contract()
    {
        using var factory = new GameApiFactory();
        using var client = factory.CreateClient();
        var document = await GetOpenApiDocument(client);
        var gameId = await StartGame(client);

        using var response = await client.PostAsJsonAsync("/api/game/guess", new
        {
            gameId,
            letter = "C"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await AssertResponsePropertiesMatchSchema(document, "/api/game/guess", "post", "200", response);
    }

    [Fact]
    public async Task Hint_response_matches_the_published_openapi_contract()
    {
        using var factory = new GameApiFactory();
        using var client = factory.CreateClient();
        var document = await GetOpenApiDocument(client);
        var gameId = await StartGame(client);

        using var response = await client.PostAsJsonAsync("/api/game/hint", new { gameId });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await AssertResponsePropertiesMatchSchema(document, "/api/game/hint", "post", "200", response);
    }

    [Fact]
    public async Task Finish_response_matches_the_published_openapi_contract()
    {
        using var factory = new GameApiFactory();
        using var client = factory.CreateClient();
        var document = await GetOpenApiDocument(client);
        var gameId = await StartGame(client);

        using var response = await client.PostAsJsonAsync("/api/game/finish", new { gameId });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await AssertResponsePropertiesMatchSchema(document, "/api/game/finish", "post", "200", response);
    }

    [Fact]
    public async Task Stop_response_matches_the_published_openapi_contract()
    {
        using var factory = new GameApiFactory();
        using var client = factory.CreateClient();
        var document = await GetOpenApiDocument(client);
        var gameId = await StartGame(client);

        using var response = await client.PostAsJsonAsync("/api/game/stop", new { gameId });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await AssertResponsePropertiesMatchSchema(document, "/api/game/stop", "post", "200", response);
    }

    private static async Task<JsonObject> GetOpenApiDocument(HttpClient client)
    {
        var document = await client.GetFromJsonAsync<JsonObject>("/openapi/v1.json");
        Assert.NotNull(document);
        Assert.StartsWith("3.1.", document["openapi"]!.GetValue<string>());
        return document;
    }

    private static async Task<string> StartGame(HttpClient client)
    {
        using var response = await client.PostAsJsonAsync("/api/game/start", new
        {
            language = "english",
            difficulty = "beginner",
            topic = "car-parts",
            playerName = "Player"
        });

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.NotNull(body);
        return body["gameId"]!.GetValue<string>();
    }

    private static async Task AssertResponsePropertiesMatchSchema(
        JsonObject document,
        string path,
        string method,
        string statusCode,
        HttpResponseMessage response)
    {
        var operation = document["paths"]![path]![method]!.AsObject();
        var responseSchema = ResolveSchema(
            document,
            operation["responses"]![statusCode]!["content"]!["application/json"]!["schema"]);

        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var actualProperties = body.RootElement.EnumerateObject()
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        var contractProperties = responseSchema["properties"]!.AsObject().Select(property => property.Key)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(contractProperties, actualProperties);
    }

    private static JsonObject ResolveSchema(JsonObject document, JsonNode? schema)
    {
        var schemaObject = Assert.IsType<JsonObject>(schema);
        var reference = schemaObject["$ref"]?.GetValue<string>();
        if (reference is null)
        {
            return schemaObject;
        }

        const string componentPrefix = "#/components/schemas/";
        Assert.StartsWith(componentPrefix, reference);
        return document["components"]!["schemas"]![reference[componentPrefix.Length..]]!.AsObject();
    }

    private sealed class GameApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<GameDbContext>();
                services.RemoveAll<DbContextOptions<GameDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<GameDbContext>>();
                services.AddDbContext<GameDbContext>(options =>
                    options.UseInMemoryDatabase(_databaseName));

                services.RemoveAll<IWordProvider>();
                services.AddSingleton<IWordProvider>(new FixedWordProvider());

                services.RemoveAll<LeaderboardClient>();
                services.AddSingleton(new LeaderboardClient(new HttpClient(new FakeLeaderboardHandler())
                {
                    BaseAddress = new Uri("http://fake-leaderboard")
                }));
            });
        }
    }

    private sealed class FakeLeaderboardHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new { qualifies = false })
            });
        }
    }

    private sealed class FixedWordProvider : IWordProvider
    {
        public Task<string> GetRandomWordAsync(string language, string difficulty, string topic) =>
            Task.FromResult("CAR");
    }
}

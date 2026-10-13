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
            });
        }
    }

    private sealed class FixedWordProvider : IWordProvider
    {
        public Task<string> GetRandomWordAsync(string language, string difficulty, string topic) =>
            Task.FromResult("CAR");
    }
}

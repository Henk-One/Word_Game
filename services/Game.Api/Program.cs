using Game.Api.Data;
using Game.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
});

// Game services - Registered as Scoped because DbWordProvider depends on DbContext lifecycle
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IWordProvider, DbWordProvider>();

// Http client to external Leaderboard.Api
builder.Services.AddHttpClient<LeaderboardClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5000");
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Game database
builder.Services.AddDbContext<GameDbContext>(options =>
{
    options.UseSqlite("Data Source=game.db");
});

var app = builder.Build();

// Run automated migrations and seed vocabulary tables safely on app initialization
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<GameDbContext>();
        await context.Database.MigrateAsync();
        await DbInitializer.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database migration or initialization runtime pipelines.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/openapi/v1.json", "Game API v1"));
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }

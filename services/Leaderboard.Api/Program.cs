using Leaderboard.Api.Data;
using Leaderboard.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
});
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<LeaderboardDbContext>(options =>
    options.UseSqlite("Data Source=leaderboard.db"));


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/openapi/v1.json", "Leaderboard API v1"));
}

app.UseHttpsRedirection();
app.UseCors();
app.MapControllers();
app.Run();

public partial class Program { }

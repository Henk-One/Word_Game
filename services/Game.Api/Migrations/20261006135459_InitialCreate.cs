using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Game.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameSessions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    PlayerName = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Language = table.Column<string>(type: "TEXT", nullable: false),
                    Difficulty = table.Column<string>(type: "TEXT", nullable: false),
                    Topic = table.Column<string>(type: "TEXT", nullable: false),
                    Word = table.Column<string>(type: "TEXT", nullable: false),
                    Revealed = table.Column<string>(type: "TEXT", nullable: false),
                    GuessedLetters = table.Column<string>(type: "TEXT", nullable: false),
                    Lives = table.Column<int>(type: "INTEGER", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false),
                    GameOver = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HighScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlayerName = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false),
                    Language = table.Column<string>(type: "TEXT", nullable: false),
                    Difficulty = table.Column<string>(type: "TEXT", nullable: false),
                    Topic = table.Column<string>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HighScores", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HighScores_PlayerName_Language_Difficulty_Topic",
                table: "HighScores",
                columns: new[] { "PlayerName", "Language", "Difficulty", "Topic" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameSessions");

            migrationBuilder.DropTable(
                name: "HighScores");
        }
    }
}

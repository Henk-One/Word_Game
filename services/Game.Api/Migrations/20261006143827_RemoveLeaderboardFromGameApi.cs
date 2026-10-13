using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Game.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLeaderboardFromGameApi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HighScores");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HighScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Difficulty = table.Column<string>(type: "TEXT", nullable: false),
                    Language = table.Column<string>(type: "TEXT", nullable: false),
                    PlayerName = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false),
                    Topic = table.Column<string>(type: "TEXT", nullable: false)
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
    }
}

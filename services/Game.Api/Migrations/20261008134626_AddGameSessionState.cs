using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Game.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddGameSessionState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BonusHintProgress",
                table: "GameSessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HintCapacity",
                table: "GameSessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.AddColumn<int>(
                name: "Hints",
                table: "GameSessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "GameSessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "LivesPerLevel",
                table: "GameSessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 11);

            migrationBuilder.AddColumn<int>(
                name: "MaxLevel",
                table: "GameSessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 10);

            migrationBuilder.AddColumn<bool>(
                name: "RepeatGuessPenalty",
                table: "GameSessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RoundActive",
                table: "GameSessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "RoundNumber",
                table: "GameSessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "SolvedWords",
                table: "GameSessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WordsPerLevel",
                table: "GameSessions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 3);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BonusHintProgress",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "HintCapacity",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "Hints",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "LivesPerLevel",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "MaxLevel",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "RepeatGuessPenalty",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "RoundActive",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "RoundNumber",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "SolvedWords",
                table: "GameSessions");

            migrationBuilder.DropColumn(
                name: "WordsPerLevel",
                table: "GameSessions");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Game.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayerSettings",
                columns: table => new
                {
                    PlayerName = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Language = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Difficulty = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Topic = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerSettings", x => x.PlayerName);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerSettings");
        }
    }
}

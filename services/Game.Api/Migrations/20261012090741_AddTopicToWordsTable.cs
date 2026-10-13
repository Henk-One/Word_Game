using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Game.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTopicToWordsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Topic",
                table: "Words",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Topic",
                table: "Words");
        }
    }
}

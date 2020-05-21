using Microsoft.EntityFrameworkCore.Migrations;

namespace SmashBotUltimate.Migrations
{
    public partial class MatchWithGuild : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "GuildId",
                table: "Matches",
                nullable: false,
                defaultValue: 0ul);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "Matches");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SmashBotUltimate.Migrations
{
    public partial class Lobby : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lobbies",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(nullable: false),
                    ChannelId = table.Column<ulong>(nullable: false),
                    OwnerId = table.Column<ulong>(nullable: false),
                    RoomId = table.Column<string>(nullable: true),
                    Password = table.Column<string>(nullable: true),
                    PublishTime = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lobbies", x => new { x.GuildId, x.ChannelId, x.OwnerId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Lobbies");
        }
    }
}

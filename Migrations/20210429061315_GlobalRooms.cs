using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SmashBotUltimate.Migrations
{
    public partial class GlobalRooms : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Global",
                table: "Lobbies",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RemovalReferenceTime",
                table: "Lobbies",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Global",
                table: "Lobbies");

            migrationBuilder.DropColumn(
                name: "RemovalReferenceTime",
                table: "Lobbies");
        }
    }
}

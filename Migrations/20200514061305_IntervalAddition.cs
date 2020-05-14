using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SmashBotUltimate.Migrations
{
    public partial class IntervalAddition : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "Interval",
                table: "Matches",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Interval",
                table: "Matches");
        }
    }
}

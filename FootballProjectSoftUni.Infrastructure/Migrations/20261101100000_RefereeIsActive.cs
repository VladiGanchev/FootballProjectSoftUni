using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballProjectSoftUni.Infrastructure.Migrations
{
    public partial class RefereeIsActive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Every existing referee is active.
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Referees",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Referees");
        }
    }
}

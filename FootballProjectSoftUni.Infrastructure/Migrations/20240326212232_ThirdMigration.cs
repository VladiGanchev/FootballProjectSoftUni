using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballProjectSoftUni.Infrastructure.Migrations
{
    public partial class ThirdMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
               name: "TeamId",
               table: "Players",
               type: "int",
               nullable: true, // Променено на true, за да позволи null стойности
               oldClrType: typeof(int),
               oldType: "int");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TeamId",
                table: "Players",
                type: "int",
                nullable: false, // Връщане на nullable на false
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}

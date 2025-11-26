using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballProjectSoftUni.Infrastructure.Migrations
{
    public partial class PartnersEntityAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Partners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partners", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "600bafb9-a73d-4489-a387-643c2b8ae96c",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "dc3068fe-6376-427e-83c7-d26aacc5015e", "AQAAAAEAACcQAAAAEECo/KX1DLkuV7Xisi6gEcFyIpGX5LQlu1q0vjIpSrzLe2nhEkFsBGAR++YtsDGWKA==", "5bc72ed8-bec9-4ad5-b28e-463ebe8760a6" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Partners");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "600bafb9-a73d-4489-a387-643c2b8ae96c",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b6afad39-75c3-4591-b545-3e4423c08c41", "AQAAAAEAACcQAAAAENy0rdv+64WonhG3wFJrNK3eksvq4ksp7MurGbbtCU0pARLJ4tPkrcnB9wttJtA1gw==", "5c3d8b0d-0658-452a-9b89-5a9f74f583c5" });
        }
    }
}

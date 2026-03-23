using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballProjectSoftUni.Infrastructure.Migrations
{
    public partial class AddSecondAndThirdPlacePrizesToTournament : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SecondPlacePrize",
                table: "Tournaments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ThirdPlacePrize",
                table: "Tournaments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "600bafb9-a73d-4489-a387-643c2b8ae96c",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "8141eafb-4cf7-45c1-996e-6789f56b4078", "AQAAAAEAACcQAAAAEFeX3hSrnRtVG8zIlcFQ6IIULeTW/nNiTs0yZckaDx2nx85q/lfMnijiEuoyyAT0kA==", "72933e06-f564-4690-a023-3ad114434412" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecondPlacePrize",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "ThirdPlacePrize",
                table: "Tournaments");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "600bafb9-a73d-4489-a387-643c2b8ae96c",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b0f6ab92-c25a-4db0-aaaa-71b2161c4137", "AQAAAAEAACcQAAAAEBDOSfLNxnkYIPctBYML/vIeWfAL5zIJhojD4nebEQjE9iYDu3swWk59Gn3+0yXWiw==", "734ab003-c886-422a-bd3f-925d908fc9ff" });
        }
    }
}

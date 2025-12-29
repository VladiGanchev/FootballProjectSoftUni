using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballProjectSoftUni.Infrastructure.Migrations
{
    public partial class MakeRefereeTournamentOptional : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RefereedTournamentsCount",
                table: "Referees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "600bafb9-a73d-4489-a387-643c2b8ae96c",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "5214a1d9-d1d6-425a-b540-6cfbccdd18f1", "AQAAAAEAACcQAAAAEIis27cBsMyOAgqUffOR19sogp/zVXDx1X8VBKpTiKBwYAxi8pMdb1Cp3HokKr2RDw==", "5dec83e8-d36d-4827-851c-84dfddcf31d6" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefereedTournamentsCount",
                table: "Referees");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "600bafb9-a73d-4489-a387-643c2b8ae96c",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "ffba4574-548e-4c45-9780-a1ddc31b79bf", "AQAAAAEAACcQAAAAEL4lLu2QZ3BloWlLzclam9eMYai5FYwD0G0p5I6zwmgBwWnmRCow9JL7/hWcN+l6yA==", "7fce81d0-f6c1-4e43-b9f5-38876d31adf4" });
        }
    }
}

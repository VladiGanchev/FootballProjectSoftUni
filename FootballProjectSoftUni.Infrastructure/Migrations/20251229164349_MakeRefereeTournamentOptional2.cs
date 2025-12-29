using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballProjectSoftUni.Infrastructure.Migrations
{
    public partial class MakeRefereeTournamentOptional2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Referees_TournamentId",
                table: "Referees");

            migrationBuilder.AlterColumn<int>(
                name: "TournamentId",
                table: "Referees",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "600bafb9-a73d-4489-a387-643c2b8ae96c",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "951a91a9-94c3-486b-8771-35e7f419de57", "AQAAAAEAACcQAAAAEPFZ/ywgJOqU0uyFI2nfr96NIo9q+/xisg3VIXdKFArID0tTXv7pCEO1JtKILs1JzQ==", "82a4193c-b2ee-4af2-baaa-de3c70046ab5" });

            migrationBuilder.CreateIndex(
                name: "IX_Referees_TournamentId",
                table: "Referees",
                column: "TournamentId",
                unique: true,
                filter: "[TournamentId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Referees_TournamentId",
                table: "Referees");

            migrationBuilder.AlterColumn<int>(
                name: "TournamentId",
                table: "Referees",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "600bafb9-a73d-4489-a387-643c2b8ae96c",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "5214a1d9-d1d6-425a-b540-6cfbccdd18f1", "AQAAAAEAACcQAAAAEIis27cBsMyOAgqUffOR19sogp/zVXDx1X8VBKpTiKBwYAxi8pMdb1Cp3HokKr2RDw==", "5dec83e8-d36d-4827-851c-84dfddcf31d6" });

            migrationBuilder.CreateIndex(
                name: "IX_Referees_TournamentId",
                table: "Referees",
                column: "TournamentId",
                unique: true);
        }
    }
}

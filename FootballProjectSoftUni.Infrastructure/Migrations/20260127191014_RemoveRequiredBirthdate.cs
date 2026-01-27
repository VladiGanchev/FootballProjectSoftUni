using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballProjectSoftUni.Infrastructure.Migrations
{
    public partial class RemoveRequiredBirthdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "600bafb9-a73d-4489-a387-643c2b8ae96c",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "824da5e6-3733-4be0-8291-f3d466af326c", "AQAAAAEAACcQAAAAEDCiQr/SGZZEBFNNxJi60nwjRfJ4GAomOxUd/pGBzReQOCMyfAouwy9gOW/0pCXEBw==", "72700b87-3ca4-4e81-af0b-3b84c0d51564" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "600bafb9-a73d-4489-a387-643c2b8ae96c",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "6bdf0e03-dd5e-4e27-b650-724e7ab7cb2e", "AQAAAAEAACcQAAAAEIE935ITWWcS7PsQfMmCCQOqxSp/bkcB2hj0NkmlDUqLwbJgG0/n2R0+9wYsb9Pi2g==", "102cca13-d804-44db-8025-b7a692f2d069" });
        }
    }
}

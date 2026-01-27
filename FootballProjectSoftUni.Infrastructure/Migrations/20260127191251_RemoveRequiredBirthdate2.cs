using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballProjectSoftUni.Infrastructure.Migrations
{
    public partial class RemoveRequiredBirthdate2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "600bafb9-a73d-4489-a387-643c2b8ae96c",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "0a71d336-bb92-45bf-a4b9-7bae8c8e5357", "AQAAAAEAACcQAAAAEP6/g/PRfu8WxpamhtMjeeUkoMiHIi/tkR2XDhBeBN4FRpdllrMWAFuExZl89fKJTg==", "c145ff08-f945-4c76-9c83-dd05bb1d4349" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "600bafb9-a73d-4489-a387-643c2b8ae96c",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "824da5e6-3733-4be0-8291-f3d466af326c", "AQAAAAEAACcQAAAAEDCiQr/SGZZEBFNNxJi60nwjRfJ4GAomOxUd/pGBzReQOCMyfAouwy9gOW/0pCXEBw==", "72700b87-3ca4-4e81-af0b-3b84c0d51564" });
        }
    }
}

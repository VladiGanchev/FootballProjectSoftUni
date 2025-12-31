using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballProjectSoftUni.Infrastructure.Migrations
{
    public partial class WinnerField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Winner",
                table: "Tournaments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "600bafb9-a73d-4489-a387-643c2b8ae96c",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "baa172ba-6d5d-4eea-ab73-d972517498c4", "AQAAAAEAACcQAAAAECCh2Mm97/FpUrm24afkdoZRnP46N326FUCT3GK4v4CD2hn7otWUsMSWUHopyOP51A==", "e64fff53-c92d-4341-b6b5-8859231a33f0" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Winner",
                table: "Tournaments");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "600bafb9-a73d-4489-a387-643c2b8ae96c",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "05f46ee0-97bb-480e-95b3-51589d050cd9", "AQAAAAEAACcQAAAAEONaDh9OcKC8wo0NieA4esgoAEn67mj4L1EqSzDhtEpz9RYb9rVgKNZGRjSwh/6e6Q==", "d83350a9-b2e4-4e8d-82a9-329e1bc17e49" });
        }
    }
}

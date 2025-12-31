using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballProjectSoftUni.Infrastructure.Migrations
{
    public partial class PrizeFeeAndEntryFee : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ParticipationFee",
                table: "Tournaments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Prize",
                table: "Tournaments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "600bafb9-a73d-4489-a387-643c2b8ae96c",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "05f46ee0-97bb-480e-95b3-51589d050cd9", "AQAAAAEAACcQAAAAEONaDh9OcKC8wo0NieA4esgoAEn67mj4L1EqSzDhtEpz9RYb9rVgKNZGRjSwh/6e6Q==", "d83350a9-b2e4-4e8d-82a9-329e1bc17e49" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParticipationFee",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "Prize",
                table: "Tournaments");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "600bafb9-a73d-4489-a387-643c2b8ae96c",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "951a91a9-94c3-486b-8771-35e7f419de57", "AQAAAAEAACcQAAAAEPFZ/ywgJOqU0uyFI2nfr96NIo9q+/xisg3VIXdKFArID0tTXv7pCEO1JtKILs1JzQ==", "82a4193c-b2ee-4af2-baaa-de3c70046ab5" });
        }
    }
}

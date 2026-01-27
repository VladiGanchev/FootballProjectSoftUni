using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballProjectSoftUni.Infrastructure.Migrations
{
    public partial class RemoveRequiredBirthdate3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "BirthDate",
                table: "Players",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "600bafb9-a73d-4489-a387-643c2b8ae96c",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "bb651cdd-eccd-4570-a8e4-f14d90a0be61", "AQAAAAEAACcQAAAAEAfNA3nUR9eS3k5yX0uYdnl3BFu7iJnPfsL8O37lLAnfdmT0lbfE4AxuT2JNGTsXog==", "4362b4e7-451d-43f8-b1e5-f265cc6c3432" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "BirthDate",
                table: "Players",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "600bafb9-a73d-4489-a387-643c2b8ae96c",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "0a71d336-bb92-45bf-a4b9-7bae8c8e5357", "AQAAAAEAACcQAAAAEP6/g/PRfu8WxpamhtMjeeUkoMiHIi/tkR2XDhBeBN4FRpdllrMWAFuExZl89fKJTg==", "c145ff08-f945-4c76-9c83-dd05bb1d4349" });
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballProjectSoftUni.Infrastructure.Migrations
{
    public partial class Declaration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "LiabilityDeclarationAccepted",
                table: "TournamentJoinPayments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LiabilityDeclarationAcceptedOnUtc",
                table: "TournamentJoinPayments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LiabilityDeclarationIp",
                table: "TournamentJoinPayments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LiabilityDeclarationTextSnapshot",
                table: "TournamentJoinPayments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "600bafb9-a73d-4489-a387-643c2b8ae96c",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "fe5d4075-b8c2-4566-a2cc-c03f0e3d0dca", "AQAAAAEAACcQAAAAECUj7KSXqNGtj5O0/7x8QAzoSXxtteAgWpZNNOGoZbfq4sOjFsIRUmjze5cAuHlSUA==", "0f0eeb4f-8788-4cc1-8d70-2160f789cd8f" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LiabilityDeclarationAccepted",
                table: "TournamentJoinPayments");

            migrationBuilder.DropColumn(
                name: "LiabilityDeclarationAcceptedOnUtc",
                table: "TournamentJoinPayments");

            migrationBuilder.DropColumn(
                name: "LiabilityDeclarationIp",
                table: "TournamentJoinPayments");

            migrationBuilder.DropColumn(
                name: "LiabilityDeclarationTextSnapshot",
                table: "TournamentJoinPayments");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "600bafb9-a73d-4489-a387-643c2b8ae96c",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "8141eafb-4cf7-45c1-996e-6789f56b4078", "AQAAAAEAACcQAAAAEFeX3hSrnRtVG8zIlcFQ6IIULeTW/nNiTs0yZckaDx2nx85q/lfMnijiEuoyyAT0kA==", "72933e06-f564-4690-a023-3ad114434412" });
        }
    }
}

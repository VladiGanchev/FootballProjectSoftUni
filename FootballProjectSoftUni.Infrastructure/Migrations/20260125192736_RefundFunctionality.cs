using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballProjectSoftUni.Infrastructure.Migrations
{
    public partial class RefundFunctionality : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "RefundAmount",
                table: "TournamentJoinPayments",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefundReason",
                table: "TournamentJoinPayments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefundedOnUtc",
                table: "TournamentJoinPayments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeRefundId",
                table: "TournamentJoinPayments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "600bafb9-a73d-4489-a387-643c2b8ae96c",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "6bdf0e03-dd5e-4e27-b650-724e7ab7cb2e", "AQAAAAEAACcQAAAAEIE935ITWWcS7PsQfMmCCQOqxSp/bkcB2hj0NkmlDUqLwbJgG0/n2R0+9wYsb9Pi2g==", "102cca13-d804-44db-8025-b7a692f2d069" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefundAmount",
                table: "TournamentJoinPayments");

            migrationBuilder.DropColumn(
                name: "RefundReason",
                table: "TournamentJoinPayments");

            migrationBuilder.DropColumn(
                name: "RefundedOnUtc",
                table: "TournamentJoinPayments");

            migrationBuilder.DropColumn(
                name: "StripeRefundId",
                table: "TournamentJoinPayments");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "600bafb9-a73d-4489-a387-643c2b8ae96c",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "15560ee4-16d7-4f19-ae35-ce83030e0957", "AQAAAAEAACcQAAAAEFNYXsHh3KW71eyLkuKhpCcGIIqb13TJbXFrMkP1/qPsiwnDYb51dArV0S80FfX1xw==", "9c436b75-e463-411b-8281-e06c70eb9bcc" });
        }
    }
}

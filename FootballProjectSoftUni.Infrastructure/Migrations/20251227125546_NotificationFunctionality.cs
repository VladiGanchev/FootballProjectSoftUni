using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballProjectSoftUni.Infrastructure.Migrations
{
    public partial class NotificationFunctionality : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ContactMessageId",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ContactMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ParentMessageId = table.Column<int>(type: "int", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IsFromAdmin = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactMessages_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContactMessages_ContactMessages_ParentMessageId",
                        column: x => x.ParentMessageId,
                        principalTable: "ContactMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "600bafb9-a73d-4489-a387-643c2b8ae96c",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "3b23bd17-6bc0-4caa-ad92-775bed5169c1", "AQAAAAEAACcQAAAAEGgsoKsHyUPvN2JL3StVkNpeNpqTLwEinEkgRFgyR7BYhHIrKTLcCjFVmj+b12zlhA==", "31f7e3f6-894a-4db3-9794-6cfcb5f8cdaa" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ContactMessageId",
                table: "Notifications",
                column: "ContactMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactMessages_ParentMessageId",
                table: "ContactMessages",
                column: "ParentMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactMessages_UserId",
                table: "ContactMessages",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_ContactMessages_ContactMessageId",
                table: "Notifications",
                column: "ContactMessageId",
                principalTable: "ContactMessages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_ContactMessages_ContactMessageId",
                table: "Notifications");

            migrationBuilder.DropTable(
                name: "ContactMessages");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_ContactMessageId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ContactMessageId",
                table: "Notifications");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "600bafb9-a73d-4489-a387-643c2b8ae96c",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "dc3068fe-6376-427e-83c7-d26aacc5015e", "AQAAAAEAACcQAAAAEECo/KX1DLkuV7Xisi6gEcFyIpGX5LQlu1q0vjIpSrzLe2nhEkFsBGAR++YtsDGWKA==", "5bc72ed8-bec9-4ad5-b28e-463ebe8760a6" });
        }
    }
}

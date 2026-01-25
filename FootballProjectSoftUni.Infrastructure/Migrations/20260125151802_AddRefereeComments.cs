using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballProjectSoftUni.Infrastructure.Migrations
{
    public partial class AddRefereeComments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RefereeComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RefereeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefereeComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefereeComments_Referees_RefereeId",
                        column: x => x.RefereeId,
                        principalTable: "Referees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "600bafb9-a73d-4489-a387-643c2b8ae96c",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "15560ee4-16d7-4f19-ae35-ce83030e0957", "AQAAAAEAACcQAAAAEFNYXsHh3KW71eyLkuKhpCcGIIqb13TJbXFrMkP1/qPsiwnDYb51dArV0S80FfX1xw==", "9c436b75-e463-411b-8281-e06c70eb9bcc" });

            migrationBuilder.CreateIndex(
                name: "IX_RefereeComments_RefereeId",
                table: "RefereeComments",
                column: "RefereeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefereeComments");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "600bafb9-a73d-4489-a387-643c2b8ae96c",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "01341344-696b-4eb3-a576-c5e59bae3935", "AQAAAAEAACcQAAAAEIgPGh6fXubuRwIXhW9RC1DLy9AwtCHzC4stS+QT0BB1QUQyLZUqKxwZ+/U2L42mjQ==", "2f1457a6-c5e7-4379-a964-47c70abcb54f" });
        }
    }
}

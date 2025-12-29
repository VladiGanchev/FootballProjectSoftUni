using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballProjectSoftUni.Infrastructure.Migrations
{
    public partial class AddRefereeRatings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RefereesRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RefereeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefereesRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefereesRatings_Referees_RefereeId",
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
                values: new object[] { "ffba4574-548e-4c45-9780-a1ddc31b79bf", "AQAAAAEAACcQAAAAEL4lLu2QZ3BloWlLzclam9eMYai5FYwD0G0p5I6zwmgBwWnmRCow9JL7/hWcN+l6yA==", "7fce81d0-f6c1-4e43-b9f5-38876d31adf4" });

            migrationBuilder.CreateIndex(
                name: "IX_RefereesRatings_RefereeId_UserId",
                table: "RefereesRatings",
                columns: new[] { "RefereeId", "UserId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefereesRatings");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "600bafb9-a73d-4489-a387-643c2b8ae96c",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "fc396f11-59df-48f1-9d7e-0044fd403d2b", "AQAAAAEAACcQAAAAEK3GDFbItjbFgihSwB3zR068W/+YIC0dWAeH2DzppoJFEjCI1POG1bCnI3BgX/APig==", "eff22f54-c319-48d3-89d4-1ff9f648578c" });
        }
    }
}

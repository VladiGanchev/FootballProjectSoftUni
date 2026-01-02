using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballProjectSoftUni.Infrastructure.Migrations
{
    public partial class AddAppStats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayersCreatedTotal = table.Column<int>(type: "int", nullable: false),
                    TeamsCreatedTotal = table.Column<int>(type: "int", nullable: false),
                    TournamentsCreatedTotal = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppStats", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AppStats",
                columns: new[] { "Id", "PlayersCreatedTotal", "TeamsCreatedTotal", "TournamentsCreatedTotal" },
                values: new object[] { 1, 0, 0, 0 });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "600bafb9-a73d-4489-a387-643c2b8ae96c",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "867488cb-0c80-4f8a-93a6-b63adf6ecd7e", "AQAAAAEAACcQAAAAEBNg/xl+yX75tcKkNtcZgY6ZhJBaCpJ9gfvzp4iCzsWbhlLj8AVxSFwv3+VJOHITXQ==", "f9a07ac1-fbbe-46d7-8938-510d33b68680" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppStats");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "600bafb9-a73d-4489-a387-643c2b8ae96c",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "cf1525f5-4b0c-4a51-ab6a-40833a77b61f", "AQAAAAEAACcQAAAAEAKdQ0ncym3+ExbP5SM22bZB21n2d1hfnx1dbk7jOpKutypLDKIOjWodOENzWO/aXA==", "7c59fceb-01a0-4df8-a11c-527851c5fb6f" });
        }
    }
}

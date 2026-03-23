using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballProjectSoftUni.Infrastructure.Migrations
{
    public partial class ClientCulture : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Culture",
                table: "TournamentJoinPayments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "600bafb9-a73d-4489-a387-643c2b8ae96c",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b0f6ab92-c25a-4db0-aaaa-71b2161c4137", "AQAAAAEAACcQAAAAEBDOSfLNxnkYIPctBYML/vIeWfAL5zIJhojD4nebEQjE9iYDu3swWk59Gn3+0yXWiw==", "734ab003-c886-422a-bd3f-925d908fc9ff" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Culture",
                table: "TournamentJoinPayments");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "600bafb9-a73d-4489-a387-643c2b8ae96c",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "f6f8482b-3c67-475f-9c27-eaa0923d6768", "AQAAAAEAACcQAAAAEBRGz04eYo5UPlF3q63AnnpyJLGsOrQ3AedfRl4eJVfRrnFaDcYSPuNxbl9VxGB+YQ==", "8a9e3d66-1a74-4f46-b3bf-6db333cc8e2c" });
        }
    }
}

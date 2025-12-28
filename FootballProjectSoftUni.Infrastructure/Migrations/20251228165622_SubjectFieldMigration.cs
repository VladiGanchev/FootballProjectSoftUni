using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballProjectSoftUni.Infrastructure.Migrations
{
    public partial class SubjectFieldMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "ContactMessages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "600bafb9-a73d-4489-a387-643c2b8ae96c",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "fc396f11-59df-48f1-9d7e-0044fd403d2b", "AQAAAAEAACcQAAAAEK3GDFbItjbFgihSwB3zR068W/+YIC0dWAeH2DzppoJFEjCI1POG1bCnI3BgX/APig==", "eff22f54-c319-48d3-89d4-1ff9f648578c" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Subject",
                table: "ContactMessages");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "600bafb9-a73d-4489-a387-643c2b8ae96c",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "3b23bd17-6bc0-4caa-ad92-775bed5169c1", "AQAAAAEAACcQAAAAEGgsoKsHyUPvN2JL3StVkNpeNpqTLwEinEkgRFgyR7BYhHIrKTLcCjFVmj+b12zlhA==", "31f7e3f6-894a-4db3-9794-6cfcb5f8cdaa" });
        }
    }
}

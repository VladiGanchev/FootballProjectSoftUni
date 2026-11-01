using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballProjectSoftUni.Infrastructure.Migrations
{
    public partial class KeepPaymentsOfDeletedTournaments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Deleting a tournament used to delete its payments too (ON DELETE CASCADE).
            // Payments are financial records: keep them and only clear the link to the tournament.
            migrationBuilder.DropForeignKey(
                name: "FK_TournamentJoinPayments_Tournaments_TournamentId",
                table: "TournamentJoinPayments");

            migrationBuilder.AlterColumn<int>(
                name: "TournamentId",
                table: "TournamentJoinPayments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "TournamentCity",
                table: "TournamentJoinPayments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TournamentStartDate",
                table: "TournamentJoinPayments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentJoinPayments_Tournaments_TournamentId",
                table: "TournamentJoinPayments",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TournamentJoinPayments_Tournaments_TournamentId",
                table: "TournamentJoinPayments");

            migrationBuilder.DropColumn(
                name: "TournamentCity",
                table: "TournamentJoinPayments");

            migrationBuilder.DropColumn(
                name: "TournamentStartDate",
                table: "TournamentJoinPayments");

            // Payments of deleted tournaments cannot point to a tournament any more.
            migrationBuilder.Sql("DELETE FROM TournamentJoinPayments WHERE TournamentId IS NULL");

            migrationBuilder.AlterColumn<int>(
                name: "TournamentId",
                table: "TournamentJoinPayments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentJoinPayments_Tournaments_TournamentId",
                table: "TournamentJoinPayments",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

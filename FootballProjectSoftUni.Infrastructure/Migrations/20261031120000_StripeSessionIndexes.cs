using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballProjectSoftUni.Infrastructure.Migrations
{
    public partial class StripeSessionIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Older orders stored "" when Stripe had not created the payment (or, while the column was required,
            // the checkout session) yet. "" is not an id, and several of them would break the unique index below.
            migrationBuilder.Sql("UPDATE TournamentJoinPayments SET StripePaymentIntentId = NULL WHERE StripePaymentIntentId = ''");
            migrationBuilder.Sql("UPDATE TournamentJoinPayments SET StripeSessionId = NULL WHERE StripeSessionId = ''");

            migrationBuilder.AlterColumn<string>(
                name: "StripeSessionId",
                table: "TournamentJoinPayments",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StripePaymentIntentId",
                table: "TournamentJoinPayments",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TournamentJoinPayments_StripePaymentIntentId",
                table: "TournamentJoinPayments",
                column: "StripePaymentIntentId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentJoinPayments_StripeSessionId",
                table: "TournamentJoinPayments",
                column: "StripeSessionId",
                unique: true,
                filter: "[StripeSessionId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TournamentJoinPayments_StripePaymentIntentId",
                table: "TournamentJoinPayments");

            migrationBuilder.DropIndex(
                name: "IX_TournamentJoinPayments_StripeSessionId",
                table: "TournamentJoinPayments");

            migrationBuilder.AlterColumn<string>(
                name: "StripeSessionId",
                table: "TournamentJoinPayments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StripePaymentIntentId",
                table: "TournamentJoinPayments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);
        }
    }
}

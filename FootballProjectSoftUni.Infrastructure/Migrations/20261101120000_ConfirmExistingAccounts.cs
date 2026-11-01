using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballProjectSoftUni.Infrastructure.Migrations
{
    public partial class ConfirmExistingAccounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Signing in now needs a confirmed e-mail. Until now nobody had to confirm, so almost no account is
            // confirmed: count the existing ones as confirmed, so that nobody (the organiser included) is locked out.
            migrationBuilder.Sql("UPDATE AspNetUsers SET EmailConfirmed = 1 WHERE EmailConfirmed = 0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Which accounts were unconfirmed before is not known any more; going back leaves them confirmed.
        }
    }
}

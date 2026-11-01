using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballProjectSoftUni.Infrastructure.Migrations
{
    public partial class NewTournamentEmails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // E-mails about new tournaments advertise a paid service, so they need consent: nobody has given it
            // yet, so the option starts off for every existing account (the notifications on the site stay).
            migrationBuilder.AddColumn<bool>(
                name: "NewTournamentEmails",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewTournamentEmails",
                table: "AspNetUsers");
        }
    }
}

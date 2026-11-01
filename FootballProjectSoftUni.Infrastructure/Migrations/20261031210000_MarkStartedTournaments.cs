using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FootballProjectSoftUni.Infrastructure.Migrations
{
    public partial class MarkStartedTournaments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Until now nothing updated Status, so every tournament was still "Upcoming" (0).
            // The start job handles tournaments with Status = Upcoming whose start time has come; mark the
            // ones that already started before this version as Started (1) / Finished (2), so the job never
            // cancels or refunds a tournament that took place before it existed.
            migrationBuilder.Sql(@"
UPDATE Tournaments
SET Status = CASE WHEN EndDate <= GETDATE() THEN 2 ELSE 1 END
WHERE Status = 0 AND StartDate <= GETDATE()");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The previous values were all 0 (Upcoming).
            migrationBuilder.Sql("UPDATE Tournaments SET Status = 0");
        }
    }
}

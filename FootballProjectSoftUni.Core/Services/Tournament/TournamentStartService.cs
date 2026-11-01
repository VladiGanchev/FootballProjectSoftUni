using FootballProjectSoftUni.Core.Contracts.Tournament;
using FootballProjectSoftUni.Core.Extensions;
using FootballProjectSoftUni.Core.Models.Tournament;
using FootballProjectSoftUni.Infrastructure.Data;
using FootballProjectSoftUni.Infrastructure.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace FootballProjectSoftUni.Core.Services.Tournament
{
    public class TournamentStartService : ITournamentStartService
    {
        /// <summary>A tournament takes place only with at least this many teams.</summary>
        public const int MinimumTeams = 8;

        // Tournaments that started before this check existed are marked as started by a migration
        // (MarkStartedTournaments). The window is an extra safety net: a tournament is handled at most a week
        // after its start (e.g. after the server was down), never long after it took place.
        private static readonly TimeSpan CatchUpWindow = TimeSpan.FromDays(7);

        private static readonly TimeSpan WarningBeforeStart = TimeSpan.FromHours(24);

        private readonly ApplicationDbContext context;
        private readonly ITournamentService tournamentService;
        private readonly ITournamentDeletionService deletionService;

        public TournamentStartService(
            ApplicationDbContext context,
            ITournamentService tournamentService,
            ITournamentDeletionService deletionService)
        {
            this.context = context;
            this.tournamentService = tournamentService;
            this.deletionService = deletionService;
        }

        public async Task<IReadOnlyList<LowTeamsWarning>> TakeLowTeamsWarningsAsync()
        {
            var now = BgTime.Now;
            var until = now.Add(WarningBeforeStart);

            var soon = await context.Tournaments
                .Where(t => t.Status == TournamentStatus.Upcoming
                         && !t.LowTeamsWarningSent
                         && t.StartDate > now
                         && t.StartDate <= until)
                .ToListAsync();

            var warnings = new List<LowTeamsWarning>();

            foreach (var tournament in soon)
            {
                var teams = await context.TournamentsTeams.CountAsync(tt => tt.TournamentId == tournament.Id);
                if (teams >= MinimumTeams)
                {
                    // Checked again next time: teams can still leave before the start.
                    continue;
                }

                tournament.LowTeamsWarningSent = true;

                warnings.Add(new LowTeamsWarning
                {
                    TournamentId = tournament.Id,
                    City = await CityOfAsync(tournament.Id),
                    StartDate = tournament.StartDate,
                    Teams = teams
                });
            }

            await context.SaveChangesAsync();
            return warnings;
        }

        public async Task<IReadOnlyList<TournamentStartResult>> StartDueTournamentsAsync()
        {
            var now = BgTime.Now;
            var since = now - CatchUpWindow;

            var due = await context.Tournaments
                .Where(t => t.Status == TournamentStatus.Upcoming && t.StartDate <= now && t.StartDate > since)
                .Select(t => new
                {
                    t.Id,
                    t.StartDate,
                    Teams = context.TournamentsTeams.Count(tt => tt.TournamentId == t.Id),
                    HasResults = context.Matches.Any(m => m.TournamentId == t.Id &&
                        (m.WinnerTeamId != null || m.Team1Goals != null || m.Team2Goals != null))
                })
                .ToListAsync();

            var results = new List<TournamentStartResult>();

            foreach (var tournament in due)
            {
                var result = new TournamentStartResult
                {
                    TournamentId = tournament.Id,
                    City = await CityOfAsync(tournament.Id),
                    StartDate = tournament.StartDate,
                    Teams = tournament.Teams
                };

                // A tournament where the organiser already entered results is never cancelled.
                if (tournament.Teams < MinimumTeams && !tournament.HasResults)
                {
                    result.Cancellation = await deletionService.CancelForTooFewTeamsAsync(tournament.Id);

                    if (result.Cancellation.Status == TournamentDeletionStatus.NotFound)
                    {
                        continue;
                    }

                    // On failure the tournament stays "upcoming", so the next run tries again.
                    result.Outcome = result.Cancellation.Status == TournamentDeletionStatus.Deleted
                        ? TournamentStartOutcome.Canceled
                        : TournamentStartOutcome.CancelFailed;
                }
                else
                {
                    await tournamentService.PrepareBracketForStartAsync(tournament.Id);

                    var entity = await context.Tournaments.FirstAsync(t => t.Id == tournament.Id);
                    entity.Status = TournamentStatus.Started;
                    await context.SaveChangesAsync();

                    result.Outcome = TournamentStartOutcome.Started;
                }

                results.Add(result);
            }

            return results;
        }

        private Task<string?> CityOfAsync(int tournamentId) =>
            context.TournamentsCities
                .Where(tc => tc.TournamentId == tournamentId)
                .Select(tc => tc.City.Name)
                .FirstOrDefaultAsync();
    }
}

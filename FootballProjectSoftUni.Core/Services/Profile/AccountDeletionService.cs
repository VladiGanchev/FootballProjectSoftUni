using FootballProjectSoftUni.Core.Contracts.Profile;
using FootballProjectSoftUni.Core.Extensions;
using FootballProjectSoftUni.Core.Models.Profile;
using FootballProjectSoftUni.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static FootballProjectSoftUni.Core.Constants.RoleConstants;

namespace FootballProjectSoftUni.Core.Services.Profile
{
    public class AccountDeletionService : IAccountDeletionService
    {
        private readonly ApplicationDbContext context;

        public AccountDeletionService(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<AccountDeletionCheck> CheckAsync(string userId)
        {
            var now = BgTime.Now;

            // The organiser owns the tournaments (deleting the account would delete or break all of them).
            var isAdmin = await context.UserRoles
                .AnyAsync(ur => ur.UserId == userId &&
                                context.Roles.Any(r => r.Id == ur.RoleId && r.Name == AdminRole));

            if (isAdmin || await context.Tournaments.AnyAsync(t => t.OrganiserId == userId))
            {
                return new AccountDeletionCheck { Block = AccountDeletionBlock.Organizer };
            }

            var teamId = await context.Coaches
                .Where(c => c.Id == userId)
                .Select(c => c.TeamId)
                .FirstOrDefaultAsync();

            if (teamId.HasValue)
            {
                var coachTournament = await context.TournamentsTeams
                    .Where(tt => tt.TeamId == teamId.Value && tt.Tournament.EndDate > now)
                    .OrderBy(tt => tt.Tournament.StartDate)
                    .Select(tt => new
                    {
                        tt.Tournament.StartDate,
                        tt.Tournament.EndDate,
                        City = tt.Tournament.TournamentCities.Select(tc => tc.City.Name).FirstOrDefault()
                    })
                    .FirstOrDefaultAsync();

                if (coachTournament != null)
                {
                    return new AccountDeletionCheck
                    {
                        Block = AccountDeletionBlock.CoachInTournament,
                        TournamentCity = coachTournament.City,
                        TournamentStart = coachTournament.StartDate,
                        TournamentEnd = coachTournament.EndDate
                    };
                }
            }

            var refereeTournament = await context.Tournaments
                .Where(t => t.EndDate > now &&
                            (t.RefereeId == userId ||
                             t.TournamentParticipants.Any(tp => tp.ParticipantId == userId && tp.Role == "Referee")))
                .OrderBy(t => t.StartDate)
                .Select(t => new
                {
                    t.StartDate,
                    t.EndDate,
                    City = t.TournamentCities.Select(tc => tc.City.Name).FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (refereeTournament != null)
            {
                return new AccountDeletionCheck
                {
                    Block = AccountDeletionBlock.RefereeInTournament,
                    TournamentCity = refereeTournament.City,
                    TournamentStart = refereeTournament.StartDate,
                    TournamentEnd = refereeTournament.EndDate
                };
            }

            return new AccountDeletionCheck { Block = AccountDeletionBlock.None };
        }

        public async Task<AccountDeletionCheck> DeleteAccountAsync(string userId)
        {
            var check = await CheckAsync(userId);
            if (!check.CanDelete)
            {
                return check;
            }

            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return check;
            }

            // All or nothing. (The in-memory test database has no transactions.)
            using var transaction = context.Database.IsRelational()
                ? await context.Database.BeginTransactionAsync()
                : null;

            // Conversations with the organisers: every message of a conversation (also the admin's
            // replies) carries the user's id. The admin's notifications about them go first (they point to them).
            var messages = await context.ContactMessages
                .Where(m => m.UserId == userId)
                .ToListAsync();
            var messageIds = messages.Select(m => m.Id).ToList();

            context.Notifications.RemoveRange(await context.Notifications
                .Where(n => n.UserId == userId ||
                            (n.ContactMessageId.HasValue && messageIds.Contains(n.ContactMessageId.Value)))
                .ToListAsync());

            context.ContactMessages.RemoveRange(messages);

            // Ratings and comments the user wrote about referees.
            context.RefereesRatings.RemoveRange(await context.RefereesRatings.Where(r => r.UserId == userId).ToListAsync());
            context.RefereeComments.RemoveRange(await context.RefereeComments.Where(c => c.UserId == userId).ToListAsync());

            await RemoveRefereeProfileAsync(userId);
            await RemoveCoachProfileAsync(userId);

            // Participations in finished tournaments (unfinished ones block the deletion above).
            context.TournamentsParticipants.RemoveRange(await context.TournamentsParticipants
                .Where(tp => tp.ParticipantId == userId)
                .ToListAsync());

            // Payments are NOT touched: they are accounting records and must be kept.

            // The account itself; its roles, logins and tokens go with it.
            context.Users.Remove(user);

            await context.SaveChangesAsync();

            if (transaction != null)
            {
                await transaction.CommitAsync();
            }

            return check;
        }

        private async Task RemoveRefereeProfileAsync(string userId)
        {
            var referee = await context.Referees.FirstOrDefaultAsync(r => r.Id == userId);
            if (referee == null)
            {
                return;
            }

            context.RefereesRatings.RemoveRange(await context.RefereesRatings.Where(r => r.RefereeId == userId).ToListAsync());
            context.RefereeComments.RemoveRange(await context.RefereeComments.Where(c => c.RefereeId == userId).ToListAsync());

            // Finished tournaments simply lose the referee's name.
            var refereedTournaments = await context.Tournaments
                .Where(t => t.RefereeId == userId)
                .ToListAsync();

            foreach (var tournament in refereedTournaments)
            {
                tournament.RefereeId = null;
            }

            context.Referees.Remove(referee);
        }

        private async Task RemoveCoachProfileAsync(string userId)
        {
            var coach = await context.Coaches.FirstOrDefaultAsync(c => c.Id == userId);
            if (coach == null)
            {
                return;
            }

            var teamId = coach.TeamId;
            context.Coaches.Remove(coach);

            if (!teamId.HasValue)
            {
                return;
            }

            // Player names are personal data of other people.
            context.Players.RemoveRange(await context.Players.Where(p => p.TeamId == teamId.Value).ToListAsync());

            var team = await context.Teams.FirstOrDefaultAsync(t => t.Id == teamId.Value);
            if (team == null)
            {
                return;
            }

            var playedBefore =
                await context.TournamentsTeams.AnyAsync(tt => tt.TeamId == team.Id) ||
                await context.CityBestTeams.AnyAsync(b => b.TeamId == team.Id) ||
                await context.Matches.AnyAsync(m => m.Team1Id == team.Id || m.Team2Id == team.Id || m.WinnerTeamId == team.Id);

            if (playedBefore)
            {
                // The name stays in old brackets and city rankings, without a coach or players.
                team.CoachId = string.Empty;
            }
            else
            {
                context.Teams.Remove(team);
            }
        }
    }
}

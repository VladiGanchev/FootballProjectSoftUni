using FootballProjectSoftUni.Core.Contracts.Profile;
using FootballProjectSoftUni.Core.Extensions;
using FootballProjectSoftUni.Core.Models.Profile;
using FootballProjectSoftUni.Core.Models.ServiceError;
using FootballProjectSoftUni.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Services.Profile
{
    public class ProfileService : IProfileService
    {
        private readonly ApplicationDbContext context;

        public ProfileService(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<ProfileViewModel?> GetProfileAsync(string userId)
        {
            var user = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return null;

            var isReferee = await context.Referees.AsNoTracking().AnyAsync(r => r.Id == userId && r.IsActive);
            var isCoach = await context.Coaches.AsNoTracking().AnyAsync(c => c.Id == userId);

            var model = new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                NewTournamentEmails = user.NewTournamentEmails,
                Role = isReferee ? "Referee" : isCoach ? "Coach" : null
            };

            if (isCoach)
            {
                var coach = await context.Coaches
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == userId);

                if (coach != null)
                {
                    model.Experience = int.TryParse(coach.Experience, out var exp) ? exp : 0;

                    if (coach.TeamId != null)
                    {
                        model.TeamName = await context.Teams
                            .AsNoTracking()
                            .Where(t => t.Id == coach.TeamId)
                            .Select(t => t.Name)
                            .FirstOrDefaultAsync();
                    }
                }
            }

            if (isReferee)
            {
                var referee = await context.Referees
                    .AsNoTracking()
                    .Include(r => r.Ratings)
                    .FirstOrDefaultAsync(r => r.Id == userId);

                model.Experience = referee?.Experience;
                // What he really refereed: finished tournaments (joining and leaving do not count).
                var now = BgTime.Now;
                model.RefereeTournamentsCount = await context.Tournaments
                    .CountAsync(t => t.RefereeId == userId && t.EndDate < now);

                if (referee?.Ratings != null && referee.Ratings.Any())
                {
                    model.RefereeRating = referee.Ratings.Average(r => r.Value);
                }
            }

            return model;
        }

        public async Task<ServiceError?> RemoveRefereeRoleAsync(string userId)
        {
            var referee = await context.Referees
                .FirstOrDefaultAsync(r => r.Id == userId && r.IsActive);

            if (referee == null)
            {
                return null;
            }

            // Like coaches: not while he referees a tournament that has not ended.
            var now = BgTime.Now;
            var current = await context.Tournaments
                .Where(t => t.EndDate > now &&
                            (t.RefereeId == userId ||
                             t.TournamentParticipants.Any(tp => tp.ParticipantId == userId && tp.Role == "Referee")))
                .OrderBy(t => t.StartDate)
                .Select(t => new { t.StartDate })
                .FirstOrDefaultAsync();

            if (current != null)
            {
                return new ServiceError
                {
                    Message = now >= current.StartDate
                        ? "You cannot stop being a referee while your tournament is in progress."
                        : "You cannot stop being a referee while you are assigned to an upcoming tournament. Leave it first."
                };
            }

            // The profile is switched off, not deleted: ratings, comments and the finished tournaments stay,
            // so stopping and starting again does not give a clean record.
            referee.IsActive = false;
            referee.TournamentId = null;

            await context.SaveChangesAsync();
            return null;
        }

        public async Task SetNewTournamentEmailsAsync(string userId, bool on)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null)
            {
                user.NewTournamentEmails = on;
                await context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsUnsubscribeKeyValidAsync(string? userId, string? key)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            var stamp = await context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.SecurityStamp)
                .FirstOrDefaultAsync();

            return UnsubscribeKey.IsValid(userId, stamp, key);
        }

        public async Task<bool> UnsubscribeAsync(string? userId, string? key)
        {
            if (!await IsUnsubscribeKeyValidAsync(userId, key))
            {
                return false;
            }

            await SetNewTournamentEmailsAsync(userId!, false);
            return true;
        }
    }
}

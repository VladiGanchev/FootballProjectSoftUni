using FootballProjectSoftUni.Core.Contracts.Profile;
using FootballProjectSoftUni.Core.Models.Profile;
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

            var isReferee = await context.Referees.AsNoTracking().AnyAsync(r => r.Id == userId);
            var isCoach = await context.Coaches.AsNoTracking().AnyAsync(c => c.Id == userId);

            var model = new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
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
                model.RefereeTournamentsCount = referee?.RefereedTournamentsCount ?? 0;

                if (referee?.Ratings != null && referee.Ratings.Any())
                {
                    model.RefereeRating = referee.Ratings.Average(r => r.Value);
                }
            }

            return model;
        }

        public async Task RemoveRefereeRoleAsync(string userId)
        {
            var referee = await context.Referees
                .FirstOrDefaultAsync(r => r.Id == userId);

            if (referee == null)
            {
                return;
            }

            var refereeParticipations = await context.TournamentsParticipants
                .Where(tp => tp.ParticipantId == userId && tp.Role == "Referee")
                .ToListAsync();

            context.TournamentsParticipants.RemoveRange(refereeParticipations);

            var tournament = await context.Tournaments
                .FirstOrDefaultAsync(t => t.RefereeId == userId);

            if (tournament != null)
            {
                tournament.RefereeId = null;
            }

            context.Referees.Remove(referee);

            await context.SaveChangesAsync();
        }
    }
}

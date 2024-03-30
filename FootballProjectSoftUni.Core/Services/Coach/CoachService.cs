using FootballProjectSoftUni.Core.Contracts.Coach;
using FootballProjectSoftUni.Core.Models.Coach;
using FootballProjectSoftUni.Core.Models.Tournament;
using FootballProjectSoftUni.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Services.Coach
{
    public class CoachService : ICoachService
    {
        private readonly ApplicationDbContext context;

        public CoachService(ApplicationDbContext _context)
        {
            context = _context;
        }

        public async Task BecomeCoachAsync(CoachViewModel model, string id)
        {
            var coach = new FootballProjectSoftUni.Infrastructure.Data.Models.Coach()
            {
                Id = id,
                Name = model.Name,
                Experience = model.Experience
            };

            context.Coaches.Add(coach);
            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<TournamentViewModel>> GetAllTournamentsToParticipateAsCoachAsync(string id)
        {
            var userId = id;

            return await context.TournamentsParticipants
                .Where(x => x.ParticipantId == userId && x.Role == "Coach")
                .Select(x => new TournamentViewModel()
                {
                    Id = x.TournamentId,
                    StartDate = x.Tournament.StartDate,
                    CityName = x.Tournament.TournamentCities.FirstOrDefault().City.Name,
                    EndDate = x.Tournament.EndDate,
                    Description = x.Tournament.Description,
                    Status = x.Tournament.Status.ToString(),
                    NumberOfTeams = x.Tournament.NumberOfTeams,
                    ImageUrl = x.Tournament.ImageUrl
                })
                .ToListAsync();
        }

        public async Task<bool> LeaveTournamentAsync(int tournamentId, string userId)
        {

            var tp = await context.TournamentsParticipants
                .Where(x => x.ParticipantId == userId && x.TournamentId == tournamentId && x.Role == "Coach")
                .FirstOrDefaultAsync();

            if (tp == null)
            {
                return false;
            }

            context.TournamentsParticipants.Remove(tp);
            await context.SaveChangesAsync();

            var coach = await context.Coaches
                .Where(x => x.Id == userId)
                .FirstOrDefaultAsync();

            if (coach == null)
            {
                return false;
            }

            var teamId = coach.TeamId;

            coach.TeamId = null;
            await context.SaveChangesAsync();

            var players = await context.Players
                .Where(x => x.TeamId == teamId)
                .ToListAsync();

            context.Players.RemoveRange(players);
            await context.SaveChangesAsync();

            var tournamentTeam = await context.TournamentsTeams
                .Where(x => x.TournamentId == tournamentId && x.TeamId == teamId)
                .FirstOrDefaultAsync();

            if (tournamentTeam == null)
            {
                return false;
            }

            context.TournamentsTeams.Remove(tournamentTeam);
            await context.SaveChangesAsync();

            var team = await context.Teams
                .Where(x => x.Id == teamId)
                .FirstOrDefaultAsync();

            if (team == null)
            {
                return false;
            }

            context.Teams.Remove(team);
            await context.SaveChangesAsync();

            var tournament = await context.Tournaments
                .Include(t => t.TournamentCities)
                .ThenInclude(tc => tc.City)
                .FirstOrDefaultAsync(t => t.Id == tournamentId);

            tournament.NumberOfTeams = await context.TournamentsTeams
                .Where(tt => tt.TournamentId == tournament.Id)
                .CountAsync();

            await context.SaveChangesAsync();

            return true;
        }
    }
}


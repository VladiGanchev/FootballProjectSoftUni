using FootballProjectSoftUni.Core.Contracts.Referee;
using FootballProjectSoftUni.Core.Models.Referee;
using FootballProjectSoftUni.Core.Models.ServiceError;
using FootballProjectSoftUni.Core.Models.Tournament;
using FootballProjectSoftUni.Infrastructure.Data;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Services.Referee
{
    public class RefereeService : IRefereeService
    {
        private readonly ApplicationDbContext context;

        public RefereeService(ApplicationDbContext _context)
        {
            context = _context;
        }

        public async Task<ServiceError> CheckForErrorsAsync(int tournamentId, string userId)
        {
            var tp = await context.TournamentsParticipants.Where(x => x.ParticipantId == userId
                && x.TournamentId == tournamentId
                && x.Role == "Coach")
               .FirstOrDefaultAsync();

            if (tp != null)
            {
                return new ServiceError()
                {
                    Message = "You cannot apply as a referee for this tournament because you already participate as a coach in it."
                };
            }

            var isRefereeToAnotherTournament = await context.Referees.Where(x => x.Id == userId).FirstOrDefaultAsync();

            if (isRefereeToAnotherTournament != null)
            {
                return new ServiceError()
                {
                    Message = "You cannot apply as a referee for this tournament because you already participate in another one as a referee. You have to leave the current tournament you are in to become a referee to another one."
                };
            }

            return null;
        }

        public async Task<IEnumerable<TournamentViewModel>> GetTournamentsAsync(string userId)
        {
            var tournaments = await context.TournamentsParticipants
              .Where(x => x.ParticipantId == userId && x.Role == "Referee")
              .Select(x => new TournamentViewModel()
              {
                  Id = x.TournamentId,
                  StartDate = x.Tournament.StartDate,
                  CityName = x.Tournament.TournamentCities.FirstOrDefault().City.Name,
                  EndDate = x.Tournament.EndDate,
                  Status = x.Tournament.Status.ToString(),
                  NumberOfTeams = x.Tournament.NumberOfTeams,
                  ImageUrl = x.Tournament.ImageUrl,
              })
              .ToListAsync();

            return tournaments;
        }

        public async Task<bool> CreateRefereeToTournamentAsync(RefereeFormViewMOdel model, int id, string userId, DateTime birthdate)
        {
            var referee = new FootballProjectSoftUni.Infrastructure.Data.Models.Referee()
            {
                Id = userId,
                Name = model.Name,
                Birthdate = birthdate,
                Experience = model.Experience,
                TournamentId = id
            };

            context.Referees.Add(referee);

            await context.SaveChangesAsync();


            var tournament = await context.Tournaments.Where(x => x.Id == id).FirstOrDefaultAsync();

            if (tournament == null)
            {
                return false;
            }

            tournament.Referee = referee;
            tournament.RefereeId = referee.Id;

            await context.SaveChangesAsync();

            TournamentParticipant tp = new TournamentParticipant()
            {
                ParticipantId = userId,
                TournamentId = id,
                Role = "Referee"
            };

            context.TournamentsParticipants.Add(tp);

            await context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> LeaveTournamentAsync(int id, string userId)
        {
            var tournamentId = id;

            var tp = await context.TournamentsParticipants.Where(x => x.ParticipantId == userId && x.TournamentId == tournamentId && x.Role == "Referee").FirstOrDefaultAsync();

            if (tp == null)
            {
                return false;
            }

            var tournament = await context.Tournaments
                .Include(t => t.TournamentCities)
                    .ThenInclude(tc => tc.City)
                .FirstOrDefaultAsync(t => t.Id == tournamentId);

            if (tournament == null)
            {
                return false;
            }

            // Вземете града, към който принадлежи турнира
            var cityId = tournament.TournamentCities.FirstOrDefault()?.CityId;


            context.TournamentsParticipants.Remove(tp);

            var referee = await context.Referees.FirstOrDefaultAsync(r => r.TournamentId == tournamentId);

            if (referee != null)
            {
                context.Referees.Remove(referee);
            }

            tournament.RefereeId = null;

            await context.SaveChangesAsync();

            return true;
        }
    }
}

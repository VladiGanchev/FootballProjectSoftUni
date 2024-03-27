using FootballProjectSoftUni.Core.Contracts.Team;
using FootballProjectSoftUni.Core.Models.ServiceError;
using FootballProjectSoftUni.Core.Models.Team;
using FootballProjectSoftUni.Infrastructure.Data;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FootballProjectSoftUni.Infrastructure.Data.Constants.DataConstants;

namespace FootballProjectSoftUni.Core.Services.Team
{
    public class TeamService : ITeamService
    {
        private readonly ApplicationDbContext context;

        public TeamService(ApplicationDbContext _context)
        {
            context = _context;
        }

        public async Task<ServiceError> CheckForErrorsAsync(int id, string userId)
        {
            var tournametsTeamsCount = await context.TournamentsTeams.Where(x => x.TournamentId == id).CountAsync();

            if (tournametsTeamsCount == TournamentNumnerOfTeamsMaxLength)
            {
                return new ServiceError()
                {
                    Message = "You cannot apply for this tournament because the maximum limit of teams has been reached."
                };
            }

            var referee = await context.Referees.Where(x => x.Id == userId).FirstOrDefaultAsync();

            if (referee != null)
            {
                var tp = await context.TournamentsParticipants.Where(x => x.TournamentId == id && x.ParticipantId == userId && x.Role == "Referee").FirstOrDefaultAsync();

                if (tp != null)
                {
                    return new ServiceError()
                    {
                        Message = "You cannot apply as a coach for this tournament because you are already a referee in it."
                    };
                }
            }


            var coach = await context.Coaches.Where(x => x.Id == userId).FirstOrDefaultAsync();

            if (coach == null)
            {
                return new ServiceError() { Message = "You need to become a coach to join a team." };
            }

            var ifHeAlreadyIsInTheTournamentAsCoach = await context.TournamentsParticipants.Where(x => x.ParticipantId == userId && x.TournamentId == id && x.Role == "Coach").FirstOrDefaultAsync();

            if (ifHeAlreadyIsInTheTournamentAsCoach != null)
            {
                return new ServiceError()
                {
                    Message = "You cannot apply second time for this tournament because you already participate as a coach in it."
                };
               
            }

            var isInAnotherTournamentAsCoach = await context.TournamentsParticipants.Where(x => x.ParticipantId == userId && x.Role == "Coach").FirstOrDefaultAsync();

            if (isInAnotherTournamentAsCoach != null)
            {
                return new ServiceError()
                {
                    Message = "You cannot apply for second tournament. You have to leave the current one in order to join a new one."
                };
            }

            return null;
        }

        public TeamRegistrationViewModel CreateModel(int id)
        {
            TeamRegistrationViewModel model = new TeamRegistrationViewModel()
            {
                TournamentId = id
            };

            return model;
        }

        public async Task<int> GetCityIdAsync(int id)
        {
           var cityId = await context.TournamentsCities.Where(x => x.TournamentId == id).Select(x => x.CityId).FirstOrDefaultAsync();

            return cityId;
        }

        public async Task<ServiceError> JoinTeamAsync(TeamRegistrationViewModel viewModel, int id, string userId)
        {

            int teamId;

            var players = new List<Player>();

            foreach (var playerViewModel in viewModel.Players)
            {
                DateTime birthdate = DateTime.Now;

                if (!DateTime.TryParseExact(playerViewModel.BirthDate, RequiredDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out birthdate))
                {
                    return new ServiceError()
                    {
                        Message = $"Invalid date, format must be {RequiredDateTimeFormat}"
                    };

                }

                var today = DateTime.Today;
                var age = today.Year - birthdate.Year;
                if (birthdate.Date > today.AddYears(-age)) age--;

                if (age < 18)
                {
                    return new ServiceError()
                    {
                        Message = "Players must be at least 18 years old to participate."
                    };
                }

                var player = new Player
                {
                    Name = playerViewModel.Name,
                    BirthDate = birthdate
                };

                players.Add(player);
            }

            foreach (var player in players)
            {
                context.Players.Add(player);
            }

            await context.SaveChangesAsync();

            var team = new FootballProjectSoftUni.Infrastructure.Data.Models.Team
            {
                Name = viewModel.TeamName,
                Players = players, 
                CoachId = userId,
                Coach = await context.Coaches.Where(x => x.Id == userId).FirstOrDefaultAsync()
            };

            context.Teams.Add(team);
            await context.SaveChangesAsync();

            teamId = team.Id;

            foreach (var player in players)
            {
                player.TeamId = teamId;
            }

            await context.SaveChangesAsync();

            var tournament = await context.Tournaments.Where(x => x.Id == id).FirstOrDefaultAsync();

            if (tournament == null)
            {
                return new ServiceError()
                {
                    Message = "BadRequest Message"
                };
            }

            var tournamentTeam = new TournamentTeam
            {
                TournamentId = id,
                TeamId = team.Id
            };

            context.TournamentsTeams.Add(tournamentTeam);



            await context.SaveChangesAsync();

            // tournament.NumberOfTeams = tournament.TournamentTeams.Count();

            tournament.NumberOfTeams = await context.TournamentsTeams.Where(tt => tt.TournamentId == tournament.Id).CountAsync();


            var tp = new TournamentParticipant()
            {
                ParticipantId = userId,
                TournamentId = id,
                Role = "Coach"
            };

            context.TournamentsParticipants.Add(tp);

            await context.SaveChangesAsync();

            return null;
        }
    }
}

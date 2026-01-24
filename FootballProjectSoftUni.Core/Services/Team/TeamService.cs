using FootballProjectSoftUni.Core.Contracts.Team;
using FootballProjectSoftUni.Core.Contracts.Tournament;
using FootballProjectSoftUni.Core.Models.ServiceError;
using FootballProjectSoftUni.Core.Models.Team;
using FootballProjectSoftUni.Core.Services.Tournament;
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
        private readonly ITournamentService tournamentService;

        public TeamService(ApplicationDbContext _context, ITournamentService _tournamentService)
        {
            context = _context;
            tournamentService = _tournamentService;
        }

        public async Task<ServiceError> CheckForErrorsAsync(int id, string userId)
        {
            // 0) Проверяваме самия турнир
            var tournament = await context.Tournaments.FindAsync(id);

            if (tournament == null)
            {
                return new ServiceError
                {
                    Message = "Tournament not found."
                };
            }

            // Не позволяваме join в вече завършен турнир
            if (DateTime.Now >= tournament.EndDate)
            {
                return new ServiceError
                {
                    Message = "You cannot join a tournament that has already finished."
                };
            }

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

            var hasActiveCoachParticipation = await context.TournamentsParticipants
                .Where(tp => tp.ParticipantId == userId && tp.Role == "Coach")
                .Join(
                    context.Tournaments,
                    tp => tp.TournamentId,
                    t => t.Id,
                    (tp, t) => t
                )
                .AnyAsync(t => t.EndDate > DateTime.Now); // турнирът още не е свършил

            if (hasActiveCoachParticipation)
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
            var coach = await context.Coaches
                .FirstOrDefaultAsync(c => c.Id == userId);

            if (coach == null)
            {
                return new ServiceError()
                {
                    Message = "You need to become a coach to join a team."
                };
            }

            var tournament = await context.Tournaments
                .FirstOrDefaultAsync(x => x.Id == id);

            if (tournament == null)
            {
                return new ServiceError()
                {
                    Message = "BadRequest Message"
                };
            }

            if (coach.TeamId != null)
            {
                var teamId = coach.TeamId.Value;

                var existingTournamentTeam = await context.TournamentsTeams
                    .FirstOrDefaultAsync(tt => tt.TournamentId == id && tt.TeamId == teamId);

                var addedTeamToTournament = false;

                if (existingTournamentTeam == null)
                {
                    var tournamentTeam = new TournamentTeam
                    {
                        TournamentId = id,
                        TeamId = teamId
                    };

                    context.TournamentsTeams.Add(tournamentTeam);
                    addedTeamToTournament = true;
                }

                var existingParticipation = await context.TournamentsParticipants
                    .FirstOrDefaultAsync(tp => tp.TournamentId == id
                                            && tp.ParticipantId == userId
                                            && tp.Role == "Coach");

                if (existingParticipation == null)
                {
                    var tp = new TournamentParticipant()
                    {
                        ParticipantId = userId,
                        TournamentId = id,
                        Role = "Coach"
                    };

                    context.TournamentsParticipants.Add(tp);
                }

                await context.SaveChangesAsync();

                if (addedTeamToTournament)
                {
                    tournament.NumberOfTeams = await context.TournamentsTeams
                        .Where(tt => tt.TournamentId == tournament.Id)
                        .CountAsync();

                    await context.SaveChangesAsync();

                    await tournamentService.GenerateBracketAsync(id);
                    await tournamentService.AssignTeamToBracketAsync(id, teamId);
                }

                return null;
            }

            if (viewModel == null)
            {
                return new ServiceError()
                {
                    Message = "NO_TEAM_YET"
                };
            }

            var nameExists = await context.Teams
                .AnyAsync(t => t.Name.ToLower() == viewModel.TeamName);

            if (nameExists)
            {
                return new ServiceError()
                {
                    Message = "A Team with the same name already exists"
                };
            }


            int teamIdNew;
            var players = new List<Player>();

            foreach (var playerViewModel in viewModel.Players)
            {
                DateTime birthdate;

                if (!DateTime.TryParseExact(playerViewModel.BirthDate, RequiredDateTimeFormat,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out birthdate))
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
                Coach = coach 
            };

            context.Teams.Add(team);
            await context.SaveChangesAsync();

            teamIdNew = team.Id;

            foreach (var player in players)
            {
                player.TeamId = teamIdNew;
            }

            await context.SaveChangesAsync();

            var tournamentTeamNew = new TournamentTeam
            {
                TournamentId = id,
                TeamId = team.Id
            };

            context.TournamentsTeams.Add(tournamentTeamNew);

            var tpNew = new TournamentParticipant()
            {
                ParticipantId = userId,
                TournamentId = id,
                Role = "Coach"
            };

            context.TournamentsParticipants.Add(tpNew);

            await context.SaveChangesAsync();

            tournament.NumberOfTeams = await context.TournamentsTeams
                .Where(tt => tt.TournamentId == tournament.Id)
                .CountAsync();

            var stats = await context.AppStats.FindAsync(1);

            if (stats != null)
            {
                stats.TeamsCreatedTotal++;
                stats.PlayersCreatedTotal += players.Count();
            }

            await context.SaveChangesAsync();

            await tournamentService.GenerateBracketAsync(id);
            await tournamentService.AssignTeamToBracketAsync(id, team.Id);

            return null;
        }

        public async Task<int> CreateTeamDraftAsync(TeamRegistrationViewModel viewModel, string userId)
        {
            var coach = await context.Coaches.FirstOrDefaultAsync(c => c.Id == userId);
            if (coach == null) throw new InvalidOperationException("Coach not found.");

            // name uniqueness
            var nameExists = await context.Teams.AnyAsync(t => t.Name.ToLower() == viewModel.TeamName.ToLower());
            if (nameExists) throw new InvalidOperationException("A Team with the same name already exists");

            var players = new List<Player>();

            foreach (var p in viewModel.Players)
            {
                if (!DateTime.TryParseExact(p.BirthDate, RequiredDateTimeFormat,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var birthdate))
                {
                    throw new InvalidOperationException($"Invalid date, format must be {RequiredDateTimeFormat}");
                }

                var today = DateTime.Today;
                var age = today.Year - birthdate.Year;
                if (birthdate.Date > today.AddYears(-age)) age--;

                if (age < 18)
                {
                    throw new InvalidOperationException("Players must be at least 18 years old to participate.");
                }

                players.Add(new Player { Name = p.Name, BirthDate = birthdate });
            }

            context.Players.AddRange(players);
            await context.SaveChangesAsync();

            var team = new FootballProjectSoftUni.Infrastructure.Data.Models.Team
            {
                Name = viewModel.TeamName,
                Players = players,
                CoachId = userId,
                Coach = coach
                // Ако имаш поле IsActive / IsDraft -> set it here
            };

            context.Teams.Add(team);
            await context.SaveChangesAsync();

            var teamId = team.Id;

            foreach (var player in players)
                player.TeamId = teamId;

            // закачи coach.TeamId = teamId (ако не го правиш другаде)
            coach.TeamId = teamId;

            // stats
            var stats = await context.AppStats.FindAsync(1);
            if (stats != null)
            {
                stats.TeamsCreatedTotal++;
                stats.PlayersCreatedTotal += players.Count;
            }

            await context.SaveChangesAsync();
            return teamId;
        }

        public async Task FinalizeJoinAsync(int tournamentId, string userId, int teamId)
        {
            var tournament = await context.Tournaments.FirstOrDefaultAsync(t => t.Id == tournamentId);
            if (tournament == null) throw new ArgumentException("Tournament not found.");

            // Не allow join в finished
            if (DateTime.Now >= tournament.EndDate)
                throw new InvalidOperationException("Tournament already finished.");

            // Ensure team exists
            var team = await context.Teams.FirstOrDefaultAsync(t => t.Id == teamId);
            if (team == null) throw new InvalidOperationException("Team not found.");

            // 1) Add team to tournament if not exists
            var existingTournamentTeam = await context.TournamentsTeams
                .FirstOrDefaultAsync(tt => tt.TournamentId == tournamentId && tt.TeamId == teamId);

            var addedTeamToTournament = false;
            if (existingTournamentTeam == null)
            {
                context.TournamentsTeams.Add(new TournamentTeam
                {
                    TournamentId = tournamentId,
                    TeamId = teamId
                });
                addedTeamToTournament = true;
            }

            // 2) Add participant coach if not exists
            var existingParticipation = await context.TournamentsParticipants
                .FirstOrDefaultAsync(tp => tp.TournamentId == tournamentId
                                        && tp.ParticipantId == userId
                                        && tp.Role == "Coach");

            if (existingParticipation == null)
            {
                context.TournamentsParticipants.Add(new TournamentParticipant
                {
                    ParticipantId = userId,
                    TournamentId = tournamentId,
                    Role = "Coach"
                });
            }

            await context.SaveChangesAsync();

            // 3) Update NumberOfTeams + bracket only if new team added
            if (addedTeamToTournament)
            {
                tournament.NumberOfTeams = await context.TournamentsTeams
                    .Where(tt => tt.TournamentId == tournamentId)
                    .CountAsync();

                await context.SaveChangesAsync();

                await tournamentService.GenerateBracketAsync(tournamentId);
                await tournamentService.AssignTeamToBracketAsync(tournamentId, teamId);
            }
        }
        public async Task<int?> GetCoachTeamIdAsync(string userId)
        {
            return await context.Coaches
                .Where(c => c.Id == userId)
                .Select(c => c.TeamId)
                .FirstOrDefaultAsync();
        }


    }
}

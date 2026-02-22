using FootballProjectSoftUni.Core.Contracts.Coach;
using FootballProjectSoftUni.Core.Contracts.Tournament;
using FootballProjectSoftUni.Core.Models.Coach;
using FootballProjectSoftUni.Core.Models.ServiceError;
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
        private readonly ITournamentService tournamentService;

        public CoachService(ApplicationDbContext _context, ITournamentService _tournamentService)
        {
            context = _context;
            tournamentService = _tournamentService;
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

        public async Task<ServiceError> CheckForErrorsAsync(string userId)
        {
            var isAlreadyACoach = await context.Coaches.AnyAsync(x => x.Id == userId);

            if (isAlreadyACoach == true)
            {
                return new ServiceError()
                {
                    Message = "You have already registered as a coach."
                };
            }

            var referee = await context.Referees
                .FirstOrDefaultAsync(r => r.Id == userId);

            if (referee != null)
            {
                return new ServiceError()
                {
                    Message = "You cannot become a coach because you are already registered as a referee in the system."
                };
            }

            return null;
        }

        public async Task<IEnumerable<TournamentViewModel>> GetAllTournamentsToParticipateAsCoachAsync(string id)
        {
            var userId = id;

            return await context.TournamentsParticipants
                .Where(x => x.ParticipantId == userId
                         && x.Role == "Coach"
                         && x.Tournament.EndDate > DateTime.Now)
                .Select(x => new TournamentViewModel()
                {
                    Id = x.TournamentId,
                    StartDate = x.Tournament.StartDate,
                    CityName = x.Tournament.TournamentCities.FirstOrDefault().City.Name,
                    EndDate = x.Tournament.EndDate,
                    Description = x.Tournament.Description,
                    Status = x.Tournament.Status,
                    NumberOfTeams = x.Tournament.NumberOfTeams,
                    ImageUrl = x.Tournament.ImageUrl
                })
                .ToListAsync();
        }

        public async Task<bool> LeaveTournamentAsync(int tournamentId, string userId)
        {
            var tournament = await context.Tournaments
                .FirstOrDefaultAsync(t => t.Id == tournamentId);

            if (tournament == null)
            {
                return false;
            }

            if (DateTime.Now >= tournament.StartDate)
            {
                return false;
            }

            var tp = await context.TournamentsParticipants
                .FirstOrDefaultAsync(x => x.ParticipantId == userId
                                          && x.TournamentId == tournamentId
                                          && x.Role == "Coach");

            if (tp == null)
            {
                return false;
            }

            context.TournamentsParticipants.Remove(tp);

            var coach = await context.Coaches
                .FirstOrDefaultAsync(x => x.Id == userId);


            var teamId = coach.TeamId;

            if (teamId != null)
            {
                var tournamentTeam = await context.TournamentsTeams
                    .FirstOrDefaultAsync(x => x.TournamentId == tournamentId
                                           && x.TeamId == teamId);

                if (tournamentTeam != null)
                {
                    context.TournamentsTeams.Remove(tournamentTeam);
                }
            }

            await context.SaveChangesAsync();

            tournament.NumberOfTeams = await context.TournamentsTeams
                .Where(tt => tt.TournamentId == tournament.Id)
                .CountAsync();

            await context.SaveChangesAsync();

            if (teamId != null)
            {
                await tournamentService.RemoveTeamFromBracketAsync(tournamentId, teamId.Value);
            }

            return true;
        }

        public async Task<bool> RemoveCoachRoleAsync(string userId)
        {
            var coach = await context.Coaches
                .FirstOrDefaultAsync(c => c.Id == userId);

            if (coach == null)
            {
                return false;
            }

            var teamId = coach.TeamId;

            var coachParticipations = await context.TournamentsParticipants
                .Where(tp => tp.ParticipantId == userId && tp.Role == "Coach")
                .Where(tp => tp.Tournament.EndDate > DateTime.UtcNow)
                .ToListAsync();

            if (coachParticipations.Any())
            {
                context.TournamentsParticipants.RemoveRange(coachParticipations);
            }

            if (teamId != null)
            {
                var activeTournamentTeams = await context.TournamentsTeams
                    .Where(tt => tt.TeamId == teamId.Value)
                    .Where(tt => tt.Tournament.EndDate > DateTime.UtcNow)
                    .ToListAsync();

                if (activeTournamentTeams.Any())
                {
                    var affectedTournamentIds = activeTournamentTeams
                        .Select(tt => tt.TournamentId)
                        .Distinct()
                        .ToList();

                    context.TournamentsTeams.RemoveRange(activeTournamentTeams);

                    var tournaments = await context.Tournaments
                        .Where(t => affectedTournamentIds.Contains(t.Id))
                        .ToListAsync();

                    foreach (var t in tournaments)
                    {
                        t.NumberOfTeams = await context.TournamentsTeams
                            .Where(tt => tt.TournamentId == t.Id && tt.TeamId != teamId.Value)
                            .CountAsync();
                    }
                }

                var hasMatches = await context.Matches
                    .AnyAsync(m => m.Team1Id == teamId.Value
                                || m.Team2Id == teamId.Value
                                || m.WinnerTeamId == teamId.Value);

                if (hasMatches)
                {
                    coach.TeamId = null;
                }
                else
                {

                    var tournamentsTeamsAll = await context.TournamentsTeams
                        .Where(tt => tt.TeamId == teamId.Value)
                        .ToListAsync();
                    if (tournamentsTeamsAll.Any())
                    {
                        context.TournamentsTeams.RemoveRange(tournamentsTeamsAll);
                    }

                    var players = await context.Players
                        .Where(p => p.TeamId == teamId.Value)
                        .ToListAsync();
                    if (players.Any())
                    {
                        context.Players.RemoveRange(players);
                    }

                    var cityBestTeams = await context.CityBestTeams
                        .Where(cbt => cbt.TeamId == teamId.Value)
                        .ToListAsync();
                    if (cityBestTeams.Any())
                    {
                        context.CityBestTeams.RemoveRange(cityBestTeams);
                    }

                    var team = await context.Teams
                        .FirstOrDefaultAsync(t => t.Id == teamId.Value);
                    if (team != null)
                    {
                        context.Teams.Remove(team);
                    }
                }
            }

            context.Coaches.Remove(coach);

            await context.SaveChangesAsync();
            return true;
        }


    }
}


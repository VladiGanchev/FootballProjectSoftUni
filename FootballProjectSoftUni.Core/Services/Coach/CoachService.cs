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
                         && x.Tournament.EndDate > DateTime.Now) // 🔥 само активни/предстоящи турнири
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

            if (coach == null)
            {
                return false;
            }

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

            // 1) Махаме участието му като Coach от всички турнири
            var coachParticipations = await context.TournamentsParticipants
                .Where(tp => tp.ParticipantId == userId && tp.Role == "Coach")
                .ToListAsync();

            if (coachParticipations.Any())
            {
                context.TournamentsParticipants.RemoveRange(coachParticipations);
            }

            // 2) Ако има отбор – чистим всичко свързано с него
            if (teamId != null)
            {
                // Всички връзки team–tournament
                var tournamentsTeams = await context.TournamentsTeams
                    .Where(tt => tt.TeamId == teamId)
                    .ToListAsync();

                if (tournamentsTeams.Any())
                {
                    var tournamentIds = tournamentsTeams
                        .Select(tt => tt.TournamentId)
                        .Distinct()
                        .ToList();

                    // Обновяваме NumberOfTeams за всеки турнир
                    var tournaments = await context.Tournaments
                        .Where(t => tournamentIds.Contains(t.Id))
                        .ToListAsync();

                    foreach (var t in tournaments)
                    {
                        t.NumberOfTeams = await context.TournamentsTeams
                            .Where(tt => tt.TournamentId == t.Id && tt.TeamId != teamId)
                            .CountAsync();
                    }

                    context.TournamentsTeams.RemoveRange(tournamentsTeams);
                }

                // Играчите от отбора
                var players = await context.Players
                    .Where(p => p.TeamId == teamId)
                    .ToListAsync();

                if (players.Any())
                {
                    context.Players.RemoveRange(players);
                }

                // Самия отбор
                var team = await context.Teams
                    .FirstOrDefaultAsync(t => t.Id == teamId);

                if (team != null)
                {
                    var cityBestTeams = await context.CityBestTeams
                        .Where(cbt => cbt.TeamId == teamId)
                        .ToListAsync();

                    if (cityBestTeams.Any())
                    {
                        context.CityBestTeams.RemoveRange(cityBestTeams);
                        await context.SaveChangesAsync();
                    }

                    context.Teams.Remove(team);
                }
            }

            // 3) Накрая трим самия coach
            context.Coaches.Remove(coach);

            await context.SaveChangesAsync();
            return true;
        }

    }
}


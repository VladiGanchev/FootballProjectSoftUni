using FootballProjectSoftUni.Core.Contracts.Tournament;
using FootballProjectSoftUni.Core.Models.City;
using FootballProjectSoftUni.Core.Models.Match;
using FootballProjectSoftUni.Core.Models.Tournament;
using FootballProjectSoftUni.Infrastructure.Data;
using FootballProjectSoftUni.Infrastructure.Data.Enums;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FootballProjectSoftUni.Infrastructure.Data.Constants.DataConstants;

namespace FootballProjectSoftUni.Core.Services.Tournament
{
    public class TournamentService : ITournamentService
    {
        private readonly ApplicationDbContext data;

        public TournamentService(ApplicationDbContext _data)
        {
            data = _data;
        }

        //public async Task<AddTournamentFormViewModel> AddTournamentToCityAsync(AddTournamentFormViewModel model, int cityId)
        //{

        //    var cities = await data.Cities.Select(x => new CityViewModel()
        //    {
        //        Id = x.Id,
        //        Name = x.Name
        //    })
        //        .ToListAsync();

        //    model.Cities = cities;

        //    return model;
        //}

        public async Task<bool> DeleteTournamentAsync(int id)
        {
            var tournament = await data.Tournaments
                .Include(t => t.TournamentCities)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tournament == null)
            {
                return false;
            }

            // 1) Разкачаме рефера (НЕ го трием)
            if (tournament.RefereeId != null)
            {
                var referee = await data.Referees
                    .FirstOrDefaultAsync(x => x.Id == tournament.RefereeId);

                if (referee != null)
                {
                    referee.TournamentId = null;
                }

                tournament.RefereeId = null;
            }

            // 2) Махаме участниците в турнира
            var tournamentParticipants = await data.TournamentsParticipants
                .Where(x => x.TournamentId == id)
                .ToListAsync();

            if (tournamentParticipants.Any())
            {
                data.TournamentsParticipants.RemoveRange(tournamentParticipants);
            }

            // 3) Махаме мачовете на този турнир
            var matches = await data.Matches
                .Where(m => m.TournamentId == id)
                .ToListAsync();

            if (matches.Any())
            {
                data.Matches.RemoveRange(matches);
            }

            // 4) Махаме връзките team–tournament (НЕ трием самите отбори)
            var tournamentTeams = await data.TournamentsTeams
                .Where(x => x.TournamentId == id)
                .ToListAsync();

            if (tournamentTeams.Any())
            {
                data.TournamentsTeams.RemoveRange(tournamentTeams);
            }

            // 5) Махаме връзките city–tournament (НЕ трием градовете)
            var tournamentCities = await data.TournamentsCities
                .Where(x => x.TournamentId == id)
                .ToListAsync();

            if (tournamentCities.Any())
            {
                data.TournamentsCities.RemoveRange(tournamentCities);
            }

            // 6) Накрая трием самия турнир
            data.Tournaments.Remove(tournament);

            await data.SaveChangesAsync();

            return true;
        }


        public async Task EditTournamentAsync(EditViewModel model, DateTime start, DateTime end)
        {
            var tournament = await data.Tournaments
         .Include(t => t.TournamentCities)
         .FirstOrDefaultAsync(t => t.Id == model.Id);

            if (tournament == null)
            {
                throw new ArgumentException("Invalid tournament ID");
            }

            tournament.Description = model.Description;
            tournament.StartDate = start;
            tournament.EndDate = end;
            tournament.ImageUrl = model.ImageUrl;
            tournament.Winner = model.Winner;

            await data.SaveChangesAsync();
        }

        public async Task<CityViewModel> FindCityAsync(int id)
        {
            var city = await data.Cities.FindAsync(id);

            if (city == null)
            {
                return null;
            }

            return new CityViewModel()
            {
                Id = city.Id,
                Name = city.Name,
                ImageUrl = city.ImageUrl
            };
        }

        public async Task<IEnumerable<CityViewModel>> GetCitiesAsync()
        {
            return await data.Cities.Select(x => new CityViewModel()
            {
                Id = x.Id,
                Name = x.Name
            }).ToListAsync();
        }

        public async Task<IEnumerable<TournamentViewModel>> GetCityTournamentsAsync(int id, bool showPast)
        {
            var city = await data.Cities
                .FirstOrDefaultAsync(x => x.Id == id);

            if (city == null)
            {
                return null;
            }

            var cityTournaments = await data.TournamentsCities
                .Where(x => x.CityId == id)
                .Select(x => new TournamentViewModel()
                {
                    Id = x.TournamentId,
                    StartDate = x.Tournament.StartDate,
                    EndDate = x.Tournament.EndDate,
                    Description = x.Tournament.Description,
                    RefereeId = x.Tournament.RefereeId,
                    Status = x.Tournament.Status.ToString(),
                    NumberOfTeams = x.Tournament.NumberOfTeams,
                    ImageUrl = x.Tournament.ImageUrl
                })
                .ToListAsync();

            foreach (var tournament in cityTournaments)
            {
                UpdateTournamentStatus(tournament);
            }

            if (showPast)
            {
                return cityTournaments
                    .Where(x => x.Status == TournamentStatus.Finished.ToString())
                    .OrderByDescending(x => x.EndDate);
            }
            else
            {
                return cityTournaments
                    .Where(x => x.Status != TournamentStatus.Finished.ToString());
            }
        }

        public async Task<DetailsViewModel> GetTournamentDetailsAsync(int id)
        {
            var needed = await data.Tournaments
                .Include(t => t.Referee)
                .Include(t => t.TournamentCities).ThenInclude(tc => tc.City)
                .Include(t => t.TournamentTeams).ThenInclude(tt => tt.Team)
                .Include(t => t.Matches)
                    .ThenInclude(m => m.Team1)
                .Include(t => t.Matches)
                    .ThenInclude(m => m.Team2)
                .Include(t => t.Matches)
                    .ThenInclude(m => m.WinnerTeam)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (needed == null)
            {
                return null;
            }

            var cityId = needed.TournamentCities.FirstOrDefault().CityId;

            var model = new DetailsViewModel()
            {
                Id = needed.Id,
                Description = needed.Description,
                StartDate = needed.StartDate,
                EndDate = needed.EndDate,
                RefereeId = needed.RefereeId,
                RefereeName = needed.Referee?.Name,
                CreatedOn = needed.CreatedOn,
                Status = needed.Status.ToString(),
                NumberOfTeams = needed.NumberOfTeams,
                CityId = cityId,
                ParticipantTeams = needed.TournamentTeams
                    .Where(tt => tt.Team != null)
                    .Select(tt => tt.Team.Name)
                    .ToList(),
                Prize = needed.Prize,
                ParticipationFee = needed.ParticipationFee,
                Winner = needed.Winner

            };

            model.Matches = needed.Matches
       .OrderBy(m => m.Round)
       .ThenBy(m => m.IndexInRound)
       .Select(m => new MatchViewModel
       {
           Id = m.Id,
           Round = m.Round,
           IndexInRound = m.IndexInRound,
           Team1Id = m.Team1Id,
           Team1Name = m.Team1 != null ? m.Team1.Name : "---",
           Team2Id = m.Team2Id,
           Team2Name = m.Team2 != null ? m.Team2.Name : "---",
           Team1Goals = m.Team1Goals,
           Team2Goals = m.Team2Goals,
           WinnerTeamId = m.WinnerTeamId,
           WinnerTeamName = m.WinnerTeam != null ? m.WinnerTeam.Name : null
       })
       .ToList();

            return model;

        }

        public async Task AddTournamentToCityAsync(AddTournamentFormViewModel model, int cityId, DateTime start, DateTime end)
        {
            var tournament = new FootballProjectSoftUni.Infrastructure.Data.Models.Tournament()
            {
                StartDate = start,
                EndDate = end,
                CreatedOn = DateTime.Now,
                Description = model.Description,
                NumberOfTeams = 0,
                ImageUrl = model.ImageUrl,
                Status = FootballProjectSoftUni.Infrastructure.Data.Enums.TournamentStatus.Upcoming,
                OrganiserId = "600bafb9-a73d-4489-a387-643c2b8ae96c",
                RefereeId = null,
                Prize = model.Prize,
                ParticipationFee = model.ParticipationFee

            };

            data.Tournaments.Add(tournament);

            await data.SaveChangesAsync();

            var cityTournament = new TournamentCity()
            {
                CityId = cityId,
                TournamentId = tournament.Id
            };

            data.TournamentsCities.Add(cityTournament);

            var stats = await data.AppStats.FindAsync(1);

            if (stats != null)
            {
                stats.TournamentsCreatedTotal++;
            }

            await data.SaveChangesAsync();
        }


        private void UpdateTournamentStatus(TournamentViewModel tournament)
        {
            if (tournament.StartDate > DateTime.Now)
            {
                tournament.Status = TournamentStatus.Upcoming.ToString();
            }
            else if (DateTime.Now > tournament.StartDate && DateTime.Now < tournament.EndDate)
            {
                tournament.Status = TournamentStatus.Started.ToString();
            }
            else if (DateTime.Now > tournament.EndDate)
            {
                tournament.Status = TournamentStatus.Finished.ToString();
            }
        }

        public async Task<EditViewModel> FindTournamentAsync(int id)
        {
            var tournament = await data.Tournaments
                .Include(t => t.TournamentCities)
                   .ThenInclude(tc => tc.City)
                   .FirstOrDefaultAsync(t => t.Id == id);

            if (tournament == null)
            {
                return null;
            }

            return new EditViewModel()
            {
                Id = tournament.Id,
                Description = tournament.Description,
                EndDate = tournament.EndDate.ToString(RequiredDateTimeFormat),
                StartDate = tournament.StartDate.ToString(RequiredDateTimeFormat),
                ImageUrl = tournament.ImageUrl,
                CreatedOn = tournament.CreatedOn.ToString(RequiredDateTimeFormat),
                Winner = tournament.Winner
            };
        }

        public async Task<FootballProjectSoftUni.Infrastructure.Data.Models.Tournament> FindTournamentByIdAsync(int id)
        {
            return await data.Tournaments
                .Include(t => t.TournamentCities)
                    .ThenInclude(tc => tc.City)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task GenerateBracketAsync(int tournamentId)
        {
            var tournament = await data.Tournaments
                .Include(t => t.Matches)
                .FirstOrDefaultAsync(t => t.Id == tournamentId);

            if (tournament == null)
            {
                throw new ArgumentException("Invalid tournament id");
            }

            if (tournament.Matches.Any())
            {
                // вече има схема
                return;
            }

            // Рунд 1 – 8 мача
            for (int i = 0; i < 8; i++)
            {
                data.Matches.Add(new Match
                {
                    TournamentId = tournamentId,
                    Round = 1,
                    IndexInRound = i
                });
            }

            // Рунд 2 – 4 мача
            for (int i = 0; i < 4; i++)
            {
                data.Matches.Add(new Match
                {
                    TournamentId = tournamentId,
                    Round = 2,
                    IndexInRound = i
                });
            }

            // Рунд 3 – 2 мача
            for (int i = 0; i < 2; i++)
            {
                data.Matches.Add(new Match
                {
                    TournamentId = tournamentId,
                    Round = 3,
                    IndexInRound = i
                });
            }

            // Рунд 4 – 1 мач (финал)
            data.Matches.Add(new Match
            {
                TournamentId = tournamentId,
                Round = 4,
                IndexInRound = 0
            });

            await data.SaveChangesAsync();
        }

        public async Task AssignTeamToBracketAsync(int tournamentId, int teamId)
        {
            // взимаме мачовете от първия рунд по ред
            var firstRoundMatches = await data.Matches
                .Where(m => m.TournamentId == tournamentId && m.Round == 1)
                .OrderBy(m => m.IndexInRound)
                .ToListAsync();

            foreach (var match in firstRoundMatches)
            {
                if (!match.Team1Id.HasValue)
                {
                    match.Team1Id = teamId;
                    await data.SaveChangesAsync();
                    return;
                }

                if (!match.Team2Id.HasValue)
                {
                    match.Team2Id = teamId;
                    await data.SaveChangesAsync();
                    return;
                }
            }

            // ако няма свободно място, можеш да хвърлиш exception или да игнорираш
            throw new InvalidOperationException("No free slots in bracket.");
        }

        public async Task MoveWinnerToNextRoundAsync(int matchId)
        {
            var match = await data.Matches.FirstOrDefaultAsync(m => m.Id == matchId);

            if (match == null || !match.WinnerTeamId.HasValue)
            {
                return;
            }

            // ако е финал – записваме winner в турнира
            if (match.Round == 4)
            {
                var tournament = await data.Tournaments.FirstOrDefaultAsync(t => t.Id == match.TournamentId);
                if (tournament != null)
                {
                    var team = await data.Teams.FindAsync(match.WinnerTeamId.Value);
                    tournament.Winner = team?.Name;
                }

                return;
            }

            int nextRound = match.Round + 1;
            int nextIndex = match.IndexInRound / 2;

            var nextMatch = await data.Matches.FirstOrDefaultAsync(m =>
                m.TournamentId == match.TournamentId &&
                m.Round == nextRound &&
                m.IndexInRound == nextIndex);

            if (nextMatch == null)
            {
                return;
            }

            if (match.IndexInRound % 2 == 0)
            {
                nextMatch.Team1Id = match.WinnerTeamId;
            }
            else
            {
                nextMatch.Team2Id = match.WinnerTeamId;
            }
        }

        public async Task RemoveTeamFromBracketAsync(int tournamentId, int teamId)
        {
            var tournament = await data.Tournaments.FirstOrDefaultAsync(t => t.Id == tournamentId);
            if (tournament == null)
            {
                return;
            }

            if (DateTime.Now >= tournament.StartDate)
            {
                return;
            }

            var matches = await data.Matches
                .Where(m => m.TournamentId == tournamentId &&
                            (m.Team1Id == teamId || m.Team2Id == teamId || m.WinnerTeamId == teamId))
                .ToListAsync();

            foreach (var match in matches)
            {
                if (match.Team1Id == teamId) match.Team1Id = null;
                if (match.Team2Id == teamId) match.Team2Id = null;
                if (match.WinnerTeamId == teamId) match.WinnerTeamId = null;

                match.Team1Goals = null;
                match.Team2Goals = null;
            }

            await data.SaveChangesAsync();
        }

    }
}

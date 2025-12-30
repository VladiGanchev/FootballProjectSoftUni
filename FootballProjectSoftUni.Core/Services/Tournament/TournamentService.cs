using FootballProjectSoftUni.Core.Contracts.Tournament;
using FootballProjectSoftUni.Core.Models.City;
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
                   .ThenInclude(tc => tc.City)
               .FirstOrDefaultAsync(t => t.Id == id);


            if (tournament == null)
            {
                return false;
            }

            if (tournament.RefereeId != null)
            {
                var referee = await data.Referees
                    .FirstOrDefaultAsync(x => x.Id == tournament.RefereeId);

                if (referee != null)
                {
                    referee.TournamentId = null; 
                }

                tournament.RefereeId = null;

                await data.SaveChangesAsync();
            }

            var city = tournament.TournamentCities.FirstOrDefault(x => x.TournamentId == id);

            if (city == null)
            {
                return false;
            }

            var tournamentCity = await data.TournamentsCities.Where(x => x.TournamentId == id && x.CityId == city.CityId).FirstOrDefaultAsync();

            if (tournamentCity == null)
            {
                return false;
            }

            data.TournamentsCities.Remove(tournamentCity);

            await data.SaveChangesAsync();

            var tournamentParticipants = await data.TournamentsParticipants.Where(x => x.TournamentId == id).ToListAsync();

            if (tournamentParticipants.Count > 0)
            {
                data.TournamentsParticipants.RemoveRange(tournamentParticipants);

                await data.SaveChangesAsync();
            }

            var teamsId = await data.TournamentsTeams.Where(x => x.TournamentId == id).Select(x => x.TeamId).ToListAsync();

            if (teamsId.Count > 0)
            {
                var coaches = await data.Coaches.ToListAsync();

                foreach (var coach in coaches)
                {
                    if (teamsId.Contains(coach.TeamId ?? 0))
                    {
                        coach.TeamId = null;
                    }
                }

                await data.SaveChangesAsync();

                var tournametsTeams = await data.TournamentsTeams.Where(x => x.TournamentId == id).ToListAsync();

                data.TournamentsTeams.RemoveRange(tournametsTeams);

                await data.SaveChangesAsync();

                var playersToRemove = await data.Players.Where(p => teamsId.Contains(p.TeamId ?? 0)).ToListAsync();

                data.Players.RemoveRange(playersToRemove);

                await data.SaveChangesAsync();

                var teamsToRemove = await data.Teams.Where(t => teamsId.Contains(t.Id)).ToListAsync();

                data.Teams.RemoveRange(teamsToRemove);

                await data.SaveChangesAsync();


            }


            data.Remove(tournament);

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

        public async Task<IEnumerable<TournamentViewModel>> GetCityTournamentsAsync(int id)
        {

            var city = await data.Cities.Where(x => x.Id == id).FirstOrDefaultAsync();

            if (city == null)
            {
                return null;
            }

            var cityTournaments = await data.TournamentsCities.Where(x => x.CityId == id).Select(x => new TournamentViewModel()
            {
                Id = x.TournamentId,
                StartDate = x.Tournament.StartDate,
                EndDate = x.Tournament.EndDate,
                Description = x.Tournament.Description,
                RefereeId = x.Tournament.RefereeId,
                Status = x.Tournament.Status.ToString(),
                NumberOfTeams = x.Tournament.NumberOfTeams,
                ImageUrl = x.Tournament.ImageUrl,


            }).ToListAsync();

            foreach (var tournament in cityTournaments)
            {
                UpdateTournamentStatus(tournament);

            }

            
            await data.SaveChangesAsync();

            return cityTournaments.Where(x => x.Status != TournamentStatus.Finished.ToString());
        }

        public async Task<DetailsViewModel> GetTournamentDetailsAsync(int id)
        {
            var needed = await data.Tournaments
               .Include(t => t.Referee)
               .Include(t => t.TournamentCities)
                   .ThenInclude(tc => tc.City)
                .Include(t => t.TournamentTeams)
                .ThenInclude(tt => tt.Team)
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
                    .ToList()

            };

            return model;

        }

        public async Task AddTournamentToCityAsync(AddTournamentFormViewModel model, int cityId, DateTime start, DateTime end)
        {
            var city = await data.Cities.Where(x => x.Id == cityId).FirstOrDefaultAsync();

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
                RefereeId = null

            };

            data.Tournaments.Add(tournament);

            await data.SaveChangesAsync();

            var cityTournament = new TournamentCity()
            {
                CityId = cityId,
                TournamentId = tournament.Id
            };

            data.TournamentsCities.Add(cityTournament);

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
                CreatedOn = tournament.CreatedOn.ToString(RequiredDateTimeFormat)
            };
        }

        public async Task<FootballProjectSoftUni.Infrastructure.Data.Models.Tournament> FindTournamentByIdAsync(int id)
        {
            return await data.Tournaments
                .Include(t => t.TournamentCities)
                    .ThenInclude(tc => tc.City)
                .FirstOrDefaultAsync(t => t.Id == id);
        }
    }
}

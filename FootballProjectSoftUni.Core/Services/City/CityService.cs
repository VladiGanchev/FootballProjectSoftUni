using FootballProjectSoftUni.Core.Contracts.City;
using FootballProjectSoftUni.Core.Contracts.Tournament;
using FootballProjectSoftUni.Core.Models.City;
using FootballProjectSoftUni.Infrastructure.Data;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;



namespace FootballProjectSoftUni.Core.Services.City
{
    public class CityService : ICityService
    {
        private readonly ApplicationDbContext data;
        private readonly ITournamentService tournamentService;
        public CityService(ApplicationDbContext _data, ITournamentService _tournamentService)
        {
            data = _data;
            tournamentService = _tournamentService;
        }

        public async Task AddCityAsync(CityViewModel model)
        {
            var city = new FootballProjectSoftUni.Infrastructure.Data.Models.City()
            {
                Name = model.Name,
                ImageUrl = model.ImageUrl
            };

            data.Cities.Add(city);

            await data.SaveChangesAsync();

        }

        public async Task<IEnumerable<CityViewModel>> AllCitiesAsync(int? page)
        {
            var pageNumber = page ?? 1;
            var pageSize = 9;

            return await data.Cities
                .Select(x => new CityViewModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    ImageUrl = x.ImageUrl
                })
                .ToPagedListAsync(pageNumber, pageSize);
        }

        public async Task<IEnumerable<CityViewModel>> AllCitiesAsync()
        {
            return await data.Cities
                .Select(x => new CityViewModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    ImageUrl = x.ImageUrl
                })
                .ToListAsync();
        }

        public async Task<DeleteCityViewModel> FindTownAsync(CityViewModel model, int id)
        {
            var city = await data.Cities.FindAsync(id);

            if (city == null)
            {
                return null;
            }

            var deleteCityViewModel = new DeleteCityViewModel
            {
                Id = city.Id,
                Name = city.Name
            };

            return deleteCityViewModel;
        }

        public async Task<IEnumerable<CityViewModel>> SearchAsync(string searchString)
        {
            var cities = await data.Cities
                .Where(x => x.Name.StartsWith(searchString))
                .Select(x => new CityViewModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    ImageUrl = x.ImageUrl
                })
                .ToListAsync();

            return cities;
        }

        public async Task<bool> DeleteCityAsync(int cityId)
        {
            var city = await data.Cities.FindAsync(cityId);
            if (city == null)
                return false;

            var tournamentIds = await data.TournamentsCities
                .Where(tc => tc.CityId == cityId)
                .Select(tc => tc.TournamentId)
                .Distinct()
                .ToListAsync();

            foreach (var tournamentId in tournamentIds)
            {
                var ok = await tournamentService.DeleteTournamentAsync(tournamentId);
                if (!ok)
                    return false;
            }

            var stillHasLinks = await data.TournamentsCities.AnyAsync(tc => tc.CityId == cityId);
            if (stillHasLinks)
                return false;

            data.Cities.Remove(city);
            await data.SaveChangesAsync();

            return true;
        }


        public async Task<List<BestTeamViewModel>> GetBestTeamsAsync(int cityId)
        {
            var bestTeams = await data.CityBestTeams
                .Where(cb => cb.CityId == cityId)
                .OrderByDescending(cb => cb.WinsInCity)
                .Select(cb => new BestTeamViewModel
                {
                    TeamId = cb.Team.Id,
                    TeamName = cb.Team.Name,
                    CoachName = cb.Team.Coach.Name,
                    WinsInCity = cb.WinsInCity
                })
                .ToListAsync();

            return bestTeams;
        }

        public async Task<UpdateCityBestTeamViewModel> GetUpdateCityBestTeamFormAsync()
        {
            var cities = await data.Cities
                .Select(c => new CityDropdownViewModel
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();

            var teams = await data.Teams
                .Select(t => new TeamDropdownViewModel
                {
                    Id = t.Id,
                    Name = t.Name
                })
                .ToListAsync();

            return new UpdateCityBestTeamViewModel
            {
                Cities = cities,
                Teams = teams
            };
        }

        public async Task IncrementTeamWinsInCityAsync(int cityId, int teamId)
        {
            var entry = await data.CityBestTeams
                .FirstOrDefaultAsync(cb => cb.CityId == cityId && cb.TeamId == teamId);

            if (entry == null)
            {
                entry = new CityBestTeam
                {
                    CityId = cityId,
                    TeamId = teamId,
                    WinsInCity = 1
                };

                data.CityBestTeams.Add(entry);
            }
            else
            {
                entry.WinsInCity += 1;
            }

            await data.SaveChangesAsync();
        }

    }
}

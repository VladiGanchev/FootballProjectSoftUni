using FootballProjectSoftUni.Core.Contracts.City;
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
        public CityService(ApplicationDbContext _data)
        {
            data = _data;
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
            var pageSize = 8;

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

        public async Task<bool> DeleteCityAsync(int id)
        {
            var city = await data.Cities.FindAsync(id);

            if (city == null)
            {
                return false;
            }

            var cityTournaments = await data.Tournaments
                .Where(x => x.TournamentCities.Any(x => x.CityId == id)).ToListAsync();

            foreach (var item in cityTournaments)
            {
                var tournament = await data.Tournaments
              .Include(t => t.TournamentCities)
                  .ThenInclude(tc => tc.City)
              .FirstOrDefaultAsync(t => t.Id == item.Id);

                if (tournament == null)
                {
                    return false;
                }

                if (tournament.RefereeId != null)
                {
                    var referee = await data.Referees.Where(x => x.TournamentId == item.Id).FirstOrDefaultAsync();

                    if (referee == null)
                    {
                        return false;
                    }

                    data.Referees.Remove(referee);

                    await data.SaveChangesAsync();

                }

                var tournamentCity = await data.TournamentsCities.Where(x => x.TournamentId == item.Id && x.CityId == id).FirstOrDefaultAsync();

                if (tournamentCity == null)
                {
                    return false;
                }

                data.TournamentsCities.Remove(tournamentCity);

                await data.SaveChangesAsync();

                var tournamentParticipants = await data.TournamentsParticipants.Where(x => x.TournamentId == item.Id).ToListAsync();

                if (tournamentParticipants != null)
                {
                    data.TournamentsParticipants.RemoveRange(tournamentParticipants);

                    await data.SaveChangesAsync();
                }

                var teamsId = await data.TournamentsTeams.Where(x => x.TournamentId == item.Id).Select(x => x.TeamId).ToListAsync();

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

                    var tournametsTeams = await data.TournamentsTeams.Where(x => x.TournamentId == item.Id).ToListAsync();

                    data.TournamentsTeams.RemoveRange(tournametsTeams);

                    await data.SaveChangesAsync();

                    var playersToRemove = await data.Players.Where(p => teamsId.Contains(p.TeamId ?? 0)).ToListAsync();

                    data.Players.RemoveRange(playersToRemove);

                    await data.SaveChangesAsync();

                    var teamsToRemove = await data.Teams.Where(t => teamsId.Contains(t.Id)).ToListAsync();

                    data.Teams.RemoveRange(teamsToRemove);

                    await data.SaveChangesAsync();

                }

                data.Tournaments.Remove(item);

                await data.SaveChangesAsync();
            }

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

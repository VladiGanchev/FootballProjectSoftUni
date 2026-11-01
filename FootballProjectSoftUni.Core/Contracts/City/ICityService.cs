using FootballProjectSoftUni.Core.Models.City;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Contracts.City
{
    public interface ICityService
    {
        Task<IEnumerable<CityViewModel>> AllCitiesAsync(int? page);
        Task<IEnumerable<CityViewModel>> AllCitiesAsync();
        Task AddCityAsync(CityViewModel model);
        Task<DeleteCityViewModel> FindTownAsync(CityViewModel model, int id);
        Task<IEnumerable<CityViewModel>> SearchAsync(string searchString);
        Task<bool> DeleteCityAsync(int id);

        /// <summary>Gives the city a new picture.</summary>
        /// <returns>The address of its old picture, or null when there is no such city.</returns>
        Task<string?> ChangeCityImageAsync(int id, string imageUrl);
        Task<List<BestTeamViewModel>> GetBestTeamsAsync(int cityId);
        Task<UpdateCityBestTeamViewModel> GetUpdateCityBestTeamFormAsync();
        Task IncrementTeamWinsInCityAsync(int cityId, int teamId);
    }
}

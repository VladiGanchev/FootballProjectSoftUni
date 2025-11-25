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
        Task<List<BestTeamViewModel>> GetBestTeamsAsync(int cityId);
    }
}

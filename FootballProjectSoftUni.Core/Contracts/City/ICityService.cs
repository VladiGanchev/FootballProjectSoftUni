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
        Task<IEnumerable<CityViewModel>> All(int? page);
        Task<bool> AddCity(CityViewModel model);
        Task<IEnumerable<CityViewModel>> DeleteCity();
        Task<DeleteCityViewModel> DeleteConfirmed(CityViewModel model, int id);
        Task<bool> DeleteConfirmed(int id);
        Task<IEnumerable<CityViewModel>> Search(string searchString);
    }
}

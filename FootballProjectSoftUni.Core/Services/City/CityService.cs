using FootballProjectSoftUni.Core.Contracts.City;
using FootballProjectSoftUni.Core.Models.City;
using FootballProjectSoftUni.Infrastructure.Data.Common;
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
        private readonly IRepository repository;

        public CityService(IRepository _repository)
        {
            repository = _repository;
        }

        public Task<bool> AddCity(CityViewModel model)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<CityViewModel>> All(int? page)
        {
            var pageNumber = page ?? 1;
            var pageSize = 8;

            return await repository.AllReadOnly<Infrastructure.Data.Models.City>()
                .Select(x => new CityViewModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    ImageUrl = x.ImageUrl
                })
                .ToPagedListAsync(pageNumber, pageSize);
        }

        public Task<IEnumerable<CityViewModel>> DeleteCity()
        {
            throw new NotImplementedException();
        }

        public Task<DeleteCityViewModel> DeleteConfirmed(CityViewModel model, int id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteConfirmed(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<CityViewModel>> Search(string searchString)
        {
            throw new NotImplementedException();
        }
    }
}

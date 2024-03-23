using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Models.City
{
    public class DeleteCityViewModelFirstForm
    {
        public List<CityViewModel> Cities { get; set; } = new List<CityViewModel>();
        public int SelectedCityId { get; set; }
    }
}

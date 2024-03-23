using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Models.City
{
    public class DeleteCityViewModel
    {
        public int Id { get; set; }
        public string Name { get; internal set; } = string.Empty;
        public string ImageUrl { get; internal set; } = string.Empty;

        public IEnumerable<CityViewModel> Cities { get; set; } = new List<CityViewModel>();
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Models.City
{
    public class UpdateCityBestTeamViewModel
    {
        [Required]
        public int CityId { get; set; }

        [Required]
        public int TeamId { get; set; }

        public IEnumerable<CityDropdownViewModel> Cities { get; set; } = new List<CityDropdownViewModel>();

        public IEnumerable<TeamDropdownViewModel> Teams { get; set; } = new List<TeamDropdownViewModel>();
    }
}

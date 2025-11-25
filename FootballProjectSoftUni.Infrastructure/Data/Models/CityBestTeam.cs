using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Infrastructure.Data.Models
{
    public class CityBestTeam
    {
        [Required]
        public int CityId { get; set; }
        public City City { get; set; } = null!;

        [Required]
        public int TeamId { get; set; }
        public Team Team { get; set; } = null!;

        public int WinsInCity { get; set; } = 0;
    }
}

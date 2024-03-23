using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Infrastructure.Data.Models
{
    public class TournamentCity
    {
        [Required]
        public int TournamentId { get; set; }

        public Tournament Tournament { get; set; } = null!;

        [Required]

        public int CityId { get; set; }

        public City City { get; set; } = null!;
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Infrastructure.Data.Models
{
    public class TournamentTeam
    {
        [Required]
        public int TournamentId { get; set; }

        public Tournament Tournament { get; set; } = null!;

        [Required]

        public int TeamId { get; set; }

        public Team Team { get; set; } = null!;
    }
}

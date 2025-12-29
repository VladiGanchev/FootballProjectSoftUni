using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Infrastructure.Data.Models
{
    public class Referee
    {
        [Key]
        public string Id { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime Birthdate { get; set; }

        [Required]
        public int Experience { get; set; }

        public int? TournamentId { get; set; }
        public Tournament? Tournament { get; set; } = null!;

        public ICollection<RefereeRating> Ratings { get; set; } = new List<RefereeRating>();

        public int RefereedTournamentsCount { get; set; }
    }
}

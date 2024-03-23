using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FootballProjectSoftUni.Infrastructure.Data.Constants.DataConstants;

namespace FootballProjectSoftUni.Infrastructure.Data.Models
{
    public class Team
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(TeamNameMaxLength)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string CoachId { get; set; } = string.Empty;

        [Required]
        public Coach Coach { get; set; } = null!;


        public ICollection<TournamentTeam> TeamTournaments { get; set; } = new List<TournamentTeam>();
        public ICollection<Player> Players { get; set; } = new List<Player>();


    }
}

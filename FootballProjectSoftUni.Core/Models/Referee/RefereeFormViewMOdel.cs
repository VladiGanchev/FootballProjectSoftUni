using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FootballProjectSoftUni.Infrastructure.Data.Constants.DataConstants;

namespace FootballProjectSoftUni.Core.Models.Referee
{
    public class RefereeFormViewMOdel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = RequireErrorMessage)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = RequireErrorMessage)]
        public string Birthdate { get; set; } = string.Empty;

        [Required(ErrorMessage = RequireErrorMessage)]
        [Range(RefereeExperienceMinYears, int.MaxValue, ErrorMessage = "You should have at least {1} years of experience to become a referee to this tournament")]
        public int Experience { get; set; }

        [Required]
        public int TournamentId { get; set; }
    }
}

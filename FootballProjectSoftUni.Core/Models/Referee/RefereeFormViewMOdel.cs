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
        [StringLength(PersonNameMaxLength, MinimumLength = PersonNameMinLength, ErrorMessage = StringLengthErrorMessage)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = RequireErrorMessage)]
        public string Birthdate { get; set; } = string.Empty;

        [Required(ErrorMessage = RequireErrorMessage)]
        [Range(RefereeExperienceMinYears, ExperienceMaxYears, ErrorMessage = "Referee experience must be between {1} and {2} years.")]
        public int Experience { get; set; }

        [Required]
        public int TournamentId { get; set; }
    }
}

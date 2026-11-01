using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FootballProjectSoftUni.Infrastructure.Data.Constants.DataConstants;

namespace FootballProjectSoftUni.Core.Models.Coach
{
    public class CoachViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = RequireErrorMessage)]
        [StringLength(PersonNameMaxLength, MinimumLength = PersonNameMinLength, ErrorMessage = StringLengthErrorMessage)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = RequireErrorMessage)]
        [Range(CoachExperienceMinYears, ExperienceMaxYears, ErrorMessage = "The coach experience must be between {1} and {2} years.")]
        public string Experience { get; set; } = string.Empty;

        [Required]
        public int TeamId { get; set; }
    }
}

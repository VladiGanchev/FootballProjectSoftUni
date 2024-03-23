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
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = RequireErrorMessage)]
        [Range(CoachExperienceMinYears, int.MaxValue, ErrorMessage = CoachExperienceErrorMessage)]
        public string Experience { get; set; } = string.Empty;

        [Required]
        public int TeamId { get; set; }
    }
}

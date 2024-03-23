using FootballProjectSoftUni.Core.Models.City;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FootballProjectSoftUni.Infrastructure.Data.Constants.DataConstants;

namespace FootballProjectSoftUni.Core.Models.Tournament
{
    public class AddTournamentFormViewModel
    {
        public int Id { get; set; }

        public int CityId { get; set; }

        [Required(ErrorMessage = RequireErrorMessage)]
        public string StartDate { get; set; } = string.Empty;


        [Required(ErrorMessage = RequireErrorMessage)]

        public string EndDate { get; set; } = string.Empty;

        [Required(ErrorMessage = RequireErrorMessage)]
        [StringLength(TournamentDescriptionMaxLength, MinimumLength = TournamentDescriptionMinLength, ErrorMessage = StringLengthErrorMessage)]
        public string Description { get; set; } = string.Empty;

        public string CreatedOn { get; set; } = string.Empty;

        [Required(ErrorMessage = RequireErrorMessage)]
        public string ImageUrl { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string Referee { get; set; } = string.Empty;

        public int NumberOfTeams { get; set; }

        public string Organiser { get; set; } = string.Empty;

        public ICollection<CityViewModel> Cities { get; set; } = new List<CityViewModel>();
    }
}

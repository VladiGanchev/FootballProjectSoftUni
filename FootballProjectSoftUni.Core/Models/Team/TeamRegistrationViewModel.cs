using FootballProjectSoftUni.Core.Models.Player;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FootballProjectSoftUni.Infrastructure.Data.Constants.DataConstants;

namespace FootballProjectSoftUni.Core.Models.Team
{
    public class TeamRegistrationViewModel : IValidatableObject
    {
        [Required(ErrorMessage = RequireErrorMessage)]
        [StringLength(TeamNameMaxLength, MinimumLength = TeamNameMinLength, ErrorMessage = StringLengthErrorMessage)]
        public string TeamName { get; set; } = string.Empty;

        public List<PlayerViewModel> Players { get; set; } = new List<PlayerViewModel>();

        public int TournamentId { get; set; }
        public TeamRegistrationViewModel()
        {
            Players = new List<PlayerViewModel>();
            for (int i = 0; i < 10; i++)
            {
                Players.Add(new PlayerViewModel());
            }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var filled = Players.Count(p => !string.IsNullOrWhiteSpace(p?.Name));

            if (filled < 6)
            {
                yield return new ValidationResult(
                    "Please enter at least 6 players."
                );
            }
        }
    }
}

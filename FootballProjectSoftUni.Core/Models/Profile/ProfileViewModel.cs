using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Models.Profile
{
    public class ProfileViewModel
    {

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string? Role { get; set; }

        public int? Experience { get; set; }

        public string? TeamName { get; set; }

        public int? RefereeTournamentsCount { get; set; }
        public double? RefereeRating { get; set; }

    }
}

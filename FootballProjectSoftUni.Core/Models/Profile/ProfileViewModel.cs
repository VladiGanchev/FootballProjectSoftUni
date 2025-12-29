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

        // "Coach", "Referee" или null
        public string? Role { get; set; }

        // Общи
        public int? Experience { get; set; }

        // За Coach
        public string? TeamName { get; set; }

        // За Referee
        public int? RefereeTournamentsCount { get; set; }
        public double? RefereeRating { get; set; }

    }
}

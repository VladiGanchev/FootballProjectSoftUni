using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Models.City
{
    public class BestTeamViewModel
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string CoachName { get; set; } = string.Empty;
        public int WinsInCity { get; set; }
    }
}

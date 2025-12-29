using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Models.Referee
{
    public class RefereeListItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Experience { get; set; }

        public int TournamentsCount { get; set; }  // изиграни турнири
        public double? AverageRating { get; set; } // средна оценка
    }

}

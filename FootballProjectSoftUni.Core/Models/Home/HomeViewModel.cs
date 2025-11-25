using Microsoft.Data.SqlClient.DataClassification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Models.Home
{
    public  class HomeViewModel
    {
        public int PlayersCount { get; set; }
        public int TournamentsCount { get; set; }
        public int TeamsCount { get; set; }
        public string YearOfFoundation { get; set; } = string.Empty;
    }
}

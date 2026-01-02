using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Infrastructure.Data.Models
{
    public class TournamentStats
    {
        public int Id { get; set; } = 1;

        public int PlayersCreatedTotal { get; set; }
        public int TeamsCreatedTotal { get; set; }
        public int TournamentsCreatedTotal { get; set; }
    }
}

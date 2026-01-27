using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Models.Match
{
    public class EnterResultViewModel
    {
        public int MatchId { get; set; }
        public int TournamentId { get; set; }

        public int? Team1Id { get; set; }
        public string Team1Name { get; set; }

        public int? Team2Id { get; set; }
        public string Team2Name { get; set; }

        public int? Team1Goals { get; set; }
        public int? Team2Goals { get; set; }

    }

}

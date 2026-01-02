using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Models.Match
{
    public class MatchViewModel
    {
        public int Id { get; set; }
        public int Round { get; set; }
        public int IndexInRound { get; set; }

        public int? Team1Id { get; set; }
        public string Team1Name { get; set; }

        public int? Team2Id { get; set; }
        public string Team2Name { get; set; }

        public int? Team1Goals { get; set; }
        public int? Team2Goals { get; set; }

        public int? WinnerTeamId { get; set; }
        public string WinnerTeamName { get; set; }
    }

}

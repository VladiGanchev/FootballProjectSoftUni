using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Infrastructure.Data.Models
{
    public class Match
    {
        public int Id { get; set; }

        public int TournamentId { get; set; }
        public Tournament Tournament { get; set; }

        public int? Team1Id { get; set; }
        public Team Team1 { get; set; }

        public int? Team2Id { get; set; }
        public Team Team2 { get; set; }

        public int? Team1Goals { get; set; }
        public int? Team2Goals { get; set; }

        public int? WinnerTeamId { get; set; }
        public Team WinnerTeam { get; set; }

        public int Round { get; set; }
        public int IndexInRound { get; set; }
    }

}

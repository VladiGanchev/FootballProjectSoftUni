using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Models.Payment
{
    public class TournamentDeclarationViewModel
    {
        public int TournamentId { get; set; }
        public int TeamId { get; set; }

        public string DeclarationText { get; set; } = string.Empty;

        public bool AcceptLiabilityDeclaration { get; set; }
    }
}

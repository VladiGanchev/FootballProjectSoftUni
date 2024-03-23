using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Infrastructure.Data.Models
{
    public class TournamentParticipant
    {
        [Required]
        public string ParticipantId { get; set; } = string.Empty;

        public IdentityUser Participant { get; set; } = null!;

        [Required]
        public int TournamentId { get; set; }
        public Tournament Tournament { get; set; } = null!;

        [Required]
        public string Role { get; set; } = string.Empty;
    }
}

using FootballProjectSoftUni.Infrastructure.Data.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static FootballProjectSoftUni.Infrastructure.Data.Constants.DataConstants;


namespace FootballProjectSoftUni.Infrastructure.Data.Models
{
    public class Tournament
    {
        public int Id { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [MaxLength(TournamentDescriptionMaxLength)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string OrganiserId { get; set; } = string.Empty;

        [Required]
        public ApplicationUser Organiser { get; set; } = null!;

        [Range(TournamentNumnerOfTeamsMinLength, TournamentNumnerOfTeamsMaxLength)]
        public int NumberOfTeams { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; }

        public string? RefereeId { get; set; } = string.Empty;

        public Referee? Referee { get; set; } = null!;

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        public TournamentStatus Status { get; set; }

        public IEnumerable<TournamentTeam> TournamentTeams { get; set; } = new List<TournamentTeam>();

        public IEnumerable<TournamentCity> TournamentCities { get; set; } = new List<TournamentCity>();

        public IEnumerable<TournamentParticipant> TournamentParticipants { get; set; } = new List<TournamentParticipant>();

        [Column(TypeName = "decimal(18,2)")]
        public decimal Prize { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ParticipationFee { get; set; }

        public string? Winner { get; set; }

        public ICollection<Match> Matches { get; set; } = new List<Match>();

    }
}

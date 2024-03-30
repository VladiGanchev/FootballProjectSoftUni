﻿using FootballProjectSoftUni.Core.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Models.Tournament
{
    public class TournamentViewModel : ITournamentModel
    {
        public int Id { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int NumberOfTeams { get; set; }

        public string CityName { get; set; } = string.Empty;

        public string? RefereeId { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Infrastructure.Data.Models
{
    public class RefereeRating
    {
        public int Id { get; set; }

        [Required]
        public string RefereeId { get; set; } = null!;
        public Referee Referee { get; set; } = null!;

        [Required]
        public string UserId { get; set; } = null!;

        [Range(1, 5)]
        public int Value { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Infrastructure.Data.Models
{
    public class Player
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public DateTime? BirthDate { get; set; }

        public int? TeamId { get; set; }

        [Required]
        public Team Team { get; set; } = null!;
    }
}

using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FootballProjectSoftUni.Infrastructure.Data.Constants.DataConstants;

namespace FootballProjectSoftUni.Infrastructure.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(UserFirstNameMaxLength)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(UserLastNameMaxLength)]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// The user asked for an e-mail when a new tournament is created in a city where they played. Off unless
        /// they turn it on: such e-mails advertise a paid service and need the person's consent.
        /// </summary>
        public bool NewTournamentEmails { get; set; }
    }
}

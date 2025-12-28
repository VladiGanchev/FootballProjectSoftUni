using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Infrastructure.Data.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public int? ContactMessageId { get; set; }

        [ForeignKey(nameof(ContactMessageId))]
        public ContactMessage? ContactMessage { get; set; }
    }
}

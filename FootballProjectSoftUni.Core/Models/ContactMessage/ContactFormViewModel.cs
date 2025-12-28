using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Models.Message
{
    public class ContactFormViewModel
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [StringLength(2000, MinimumLength = 5)]
        public string Content { get; set; } = string.Empty;
    }
}

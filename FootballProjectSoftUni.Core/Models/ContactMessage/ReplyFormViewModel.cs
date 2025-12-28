using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Models.Message
{
    public class ReplyFormViewModel
    {
        [Required]
        public int ParentMessageId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Subject { get; set; } = string.Empty;

        public string CurrentMessageText { get; set; } = string.Empty;

        public string PreviousMessageText { get; set; } = string.Empty;

        [Required]
        [StringLength(2000, MinimumLength = 1)]
        public string Content { get; set; } = string.Empty;

        public bool CanReply { get; set; }
    }
}

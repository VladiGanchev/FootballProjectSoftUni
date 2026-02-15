using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Models.ContactMessage
{
    public class SentMessageViewModel
    {
        public int Id { get; set; }

        public string Subject { get; set; } = string.Empty;

        public string Preview { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; }

        public string ToName { get; set; } = string.Empty;
    }
}

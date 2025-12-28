using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Models.ContactMessage
{
    public class MessageListViewModel
    {
        public int Id { get; set; }

        public string Subject { get; set; } = string.Empty;

        public string Preview { get; set; } = string.Empty;

        public DateTime CreatedOn { get; set; }

        public bool IsFromAdmin { get; set; }
    }
}

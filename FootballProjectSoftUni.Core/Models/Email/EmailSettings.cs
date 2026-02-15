using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Models.Email
{
    public class EmailSettings
    {
        public string From { get; set; } = null!;
        public string SmtpHost { get; set; } = null!;
        public int SmtpPort { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;
        public string SmtpUser { get; set; } = null!;
        public string SmtpPass { get; set; } = null!;
        public string? OverrideTo { get; set; }
    }

}

using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Models.Referee
{
    public class RefereeInviteFormViewModel
    {
        public string RefereeId { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public IEnumerable<SelectListItem> Referees { get; set; } = new List<SelectListItem>();
    }
}

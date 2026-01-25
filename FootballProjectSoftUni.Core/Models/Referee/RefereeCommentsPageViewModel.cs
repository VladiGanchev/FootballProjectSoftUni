using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Models.Referee
{
    public class RefereeCommentsPageViewModel
    {
        public string RefereeId { get; set; } = null!;
        public string RefereeName { get; set; } = null!;
        public List<RefereeCommentViewModel> Comments { get; set; } = new();
        public string NewComment { get; set; } = string.Empty;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Models.Referee
{
    public class RefereeCommentViewModel
    {
        public int Id { get; set; }

        /// <summary>The author (not shown; tells the page which comments are the visitor's own).</summary>
        public string UserId { get; set; } = string.Empty;

        public string UserName { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime CreatedOn { get; set; }

        // Filled for a single comment (report page, deletion).
        public string RefereeId { get; set; } = string.Empty;
        public string RefereeName { get; set; } = string.Empty;
    }
}

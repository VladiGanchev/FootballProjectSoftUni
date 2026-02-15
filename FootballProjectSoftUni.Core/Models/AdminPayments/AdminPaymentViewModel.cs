using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Models.AdminPayments
{
    public class AdminPaymentViewModel
    {
        public int Id { get; set; } 
        public string UserId { get; set; } = null!;
        public int TournamentId { get; set; }
        public int? TeamId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? PaidOnUtc { get; set; }
    }
}

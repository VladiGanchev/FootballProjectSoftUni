using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Infrastructure.Data.Models
{
    public class TournamentJoinPayment
    {
        public int Id { get; set; }

        public int TournamentId { get; set; }
        public Tournament Tournament { get; set; } = null!;

        public string UserId { get; set; } = null!; 

        public int? TeamId { get; set; } 

        public string? StripeSessionId { get; set; } 
        public string? StripePaymentIntentId { get; set; }

        public decimal Amount { get; set; } 
        public string Currency { get; set; } = "eur";

        public string Status { get; set; } = "Pending"; 
        public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
        public DateTime? PaidOnUtc { get; set; }
        public string? StripeRefundId { get; set; }
        public DateTime? RefundedOnUtc { get; set; }
        public decimal? RefundAmount { get; set; }
        public string? RefundReason { get; set; }
    }
}

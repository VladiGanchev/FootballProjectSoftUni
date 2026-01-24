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

        public int? TeamId { get; set; } // ако вече имаш team или след като го създадеш (draft)

        public string? StripeSessionId { get; set; } 
        public string? StripePaymentIntentId { get; set; }

        public decimal Amount { get; set; } // participation fee (за лог)
        public string Currency { get; set; } = "eur";

        public string Status { get; set; } = "Pending"; // Pending/Paid/Failed/Expired
        public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
        public DateTime? PaidOnUtc { get; set; }
    }
}

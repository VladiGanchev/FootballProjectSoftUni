using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Models.TournamentJoinPayment
{
    public class TournamentJoinPaymentViewModel
    {
        public int Id { get; set; }
        public string Status { get; set; } = null!;
        public int TournamentId { get; set; }
        public int? TeamId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public DateTime CreatedOnUtc { get; set; }
        public DateTime? PaidOnUtc { get; set; }
        public string StripePaymentIntentId { get; set; } = null!;
    }
}

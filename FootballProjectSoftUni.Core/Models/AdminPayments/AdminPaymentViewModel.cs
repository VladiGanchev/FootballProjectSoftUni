using FootballProjectSoftUni.Core.Extensions;
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
        /// <summary>Null when the tournament was deleted (the payment is kept as a record).</summary>
        public int? TournamentId { get; set; }

        /// <summary>City of a deleted tournament, saved when it was deleted.</summary>
        public string? DeletedTournamentCity { get; set; }

        public bool TournamentDeleted => TournamentId == null;

        /// <summary>Paid, but the team left the tournament (the fee is not returned automatically).</summary>
        public bool TeamLeft { get; set; }
        public int? TeamId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? PaidOnUtc { get; set; }

        /// <summary>Refunds are allowed only before this moment.</summary>
        public DateTime? TournamentStartDate { get; set; }

        public bool CanRefund =>
            (Status == "Paid" || Status == "RefundFailed") &&
            (TournamentStartDate == null || BgTime.Now < TournamentStartDate.Value);
    }
}

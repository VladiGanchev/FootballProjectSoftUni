using FootballProjectSoftUni.Core.Contracts;
using FootballProjectSoftUni.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Models.AdminPayments
{
    public class AdminPaymentViewModel : ITournamentModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;

        /// <summary>Null when the account was deleted (the payment stays as an accounting record).</summary>
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }

        public string? TeamName { get; set; }

        /// <summary>The tournament's name (for the link to it); empty when it was deleted.</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>The tournament's city: its current one, or the one saved when it was deleted.</summary>
        public string? City { get; set; }
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

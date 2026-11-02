using FootballProjectSoftUni.Core.Models.AdminPayments;
using FootballProjectSoftUni.Core.Models.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Contracts.Payment
{
    public interface IPaymentService
    {
        Task<string> CreateTournamentJoinCheckoutAsync(int tournamentId, string userId, int? teamId, bool liabilityDeclarationAccepted, string declarationTextSnapshot);
        Task<CheckoutPaymentResult> ConfirmCheckoutPaymentAsync(CompletedCheckout checkout);
        Task ExpireOpenCheckoutsAsync(int tournamentId);

        /// <summary>
        /// A paid (not refunded) order of this user for this tournament - e.g. the coach left and comes back.
        /// It is moved to the given team; returns its id, or null when there is none.
        /// </summary>
        Task<int?> ReusePaidOrderAsync(int tournamentId, string userId, int teamId);

        /// <summary>The tournament, team and fee shown before payment; null when the tournament doesn't exist.</summary>
        Task<JoinSummaryViewModel?> GetJoinSummaryAsync(int tournamentId, string userId, int teamId);
        Task<bool> RefundTournamentJoinAsync(int orderId, decimal? amount = null, string? reason = null, bool ignoreTournamentStart = false);
        Task<AdminPaymentViewModel?> GetPaymentAsync(int orderId);
        /// <summary>A page of 25 payments for the admin, newest first, with an optional status filter and search.</summary>
        Task<AdminPaymentsPage> GetTournamentJoinPaymentsAsync(int? page, string? status, string? search);

    }
}

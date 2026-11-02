using FootballProjectSoftUni.Core.Contracts.Payment;
using FootballProjectSoftUni.Core.Models.AdminPayments;
using FootballProjectSoftUni.Core.Models.Payment;
using FootballProjectSoftUni.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FootballProjectSoftUni.Tests.Mocks
{
    /// <summary>Stands in for Stripe: refunds succeed unless the order is listed in FailingOrders.</summary>
    public class FakePaymentService : IPaymentService
    {
        private readonly ApplicationDbContext data;

        public FakePaymentService(ApplicationDbContext data) => this.data = data;

        public HashSet<int> FailingOrders { get; } = new();

        public List<int> RefundedOrders { get; } = new();

        /// <summary>Orders refunded with ignoreTournamentStart = true (allowed after the start time).</summary>
        public List<int> RefundedAfterStart { get; } = new();

        public List<int> ExpiredTournaments { get; } = new();

        public async Task<bool> RefundTournamentJoinAsync(int orderId, decimal? amount = null, string? reason = null, bool ignoreTournamentStart = false)
        {
            if (FailingOrders.Contains(orderId))
            {
                throw new InvalidOperationException("Stripe declined the refund.");
            }

            var order = await data.TournamentJoinPayments.FirstAsync(o => o.Id == orderId);
            order.Status = "Refunded";
            await data.SaveChangesAsync();

            RefundedOrders.Add(orderId);
            if (ignoreTournamentStart)
            {
                RefundedAfterStart.Add(orderId);
            }

            return true;
        }

        public Task ExpireOpenCheckoutsAsync(int tournamentId)
        {
            ExpiredTournaments.Add(tournamentId);
            return Task.CompletedTask;
        }

        public Task<int?> ReusePaidOrderAsync(int tournamentId, string userId, int teamId) =>
            throw new NotImplementedException();

        public Task<JoinSummaryViewModel?> GetJoinSummaryAsync(int tournamentId, string userId, int teamId) =>
            throw new NotImplementedException();

        public Task<string> CreateTournamentJoinCheckoutAsync(int tournamentId, string userId, int? teamId, bool liabilityDeclarationAccepted, string declarationTextSnapshot) =>
            throw new NotImplementedException();

        public Task<CheckoutPaymentResult> ConfirmCheckoutPaymentAsync(CompletedCheckout checkout) =>
            throw new NotImplementedException();

        public Task<AdminPaymentViewModel?> GetPaymentAsync(int orderId) =>
            throw new NotImplementedException();

        public Task<AdminPaymentsPage> GetTournamentJoinPaymentsAsync(int? page, string? status, string? search) =>
            throw new NotImplementedException();
    }
}

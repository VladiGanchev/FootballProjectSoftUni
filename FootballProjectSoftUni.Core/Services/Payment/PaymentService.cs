using FootballProjectSoftUni.Core.Contracts.Payment;
using FootballProjectSoftUni.Core.Extensions;
using FootballProjectSoftUni.Core.Models.AdminPayments;
using FootballProjectSoftUni.Core.Models.Payment;
using FootballProjectSoftUni.Core.Models.Settings;
using FootballProjectSoftUni.Infrastructure.Data;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Services.Payment
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext context;
        private readonly StripeSettings settings;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly SiteSettings site;

        public PaymentService(
            ApplicationDbContext context,
            IOptions<StripeSettings> options,
            IHttpContextAccessor httpContextAccessor)
            : this(context, options, httpContextAccessor, Options.Create(new SiteSettings()))
        {
        }

        public PaymentService(
            ApplicationDbContext context,
            IOptions<StripeSettings> options,
            IHttpContextAccessor httpContextAccessor,
            IOptions<SiteSettings> siteOptions)
        {
            this.context = context;
            this.settings = options.Value;
            this.httpContextAccessor = httpContextAccessor;
            this.site = siteOptions.Value;
        }

        public async Task<string> CreateTournamentJoinCheckoutAsync(int tournamentId, string userId, int? teamId, bool liabilityDeclarationAccepted, string declarationTextSnapshot)
        {
            if (!liabilityDeclarationAccepted)
            {
                throw new InvalidOperationException("Liability declaration must be accepted before payment.");
            }

            var tournament = await context.Tournaments.FirstOrDefaultAsync(t => t.Id == tournamentId);
            if (tournament == null)
            {
                throw new ArgumentException("Tournament not found.");
            }

            var fee = tournament.ParticipationFee; 
            if (fee <= 0)
            {
                throw new InvalidOperationException("Participation fee is not configured.");
            }

            long amount = (long)Math.Round(fee * 100m, MidpointRounding.AwayFromZero);
            if (amount <= 0) throw new InvalidOperationException("Invalid fee amount.");

            // One payment per user and tournament: never charge twice (two tabs, double click, back button).
            var alreadyPaid = await context.TournamentJoinPayments.AnyAsync(o =>
                o.TournamentId == tournamentId && o.UserId == userId && o.Status == "Paid");
            if (alreadyPaid)
            {
                throw new InvalidOperationException("You have already paid for this tournament.");
            }

            // The organiser is returning an earlier payment; a new one can be made once Stripe has finished.
            var refundInProgress = await context.TournamentJoinPayments.AnyAsync(o =>
                o.TournamentId == tournamentId && o.UserId == userId && o.Status == "RefundPending");
            if (refundInProgress)
            {
                throw new InvalidOperationException("Your previous payment for this tournament is still being refunded.");
            }

            var pendingOrders = await context.TournamentJoinPayments
                .Where(o => o.TournamentId == tournamentId && o.UserId == userId && o.Status == "Pending")
                .OrderByDescending(o => o.Id)
                .ToListAsync();

            var pendingSessionService = new SessionService();
            foreach (var pending in pendingOrders)
            {
                if (string.IsNullOrWhiteSpace(pending.StripeSessionId))
                {
                    pending.Status = "Expired";
                    continue;
                }

                var existing = await pendingSessionService.GetAsync(pending.StripeSessionId);

                if (existing.Status == "open" && pending.TeamId == teamId)
                {
                    // Reuse the checkout that is already open instead of creating a second charge.
                    await context.SaveChangesAsync();
                    return existing.Url;
                }

                if (existing.Status == "complete")
                {
                    throw new InvalidOperationException("Your payment for this tournament is already being processed.");
                }

                if (existing.Status == "open")
                {
                    await pendingSessionService.ExpireAsync(pending.StripeSessionId);
                }

                pending.Status = "Expired";
            }

            await context.SaveChangesAsync();

            var ip =httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            var order = new FootballProjectSoftUni.Infrastructure.Data.Models.TournamentJoinPayment
            {
                TournamentId = tournamentId,
                UserId = userId,
                TeamId = teamId,
                Amount = fee,
                Currency = settings.Currency,
                Status = "Pending",
                Culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName,
                LiabilityDeclarationAccepted = liabilityDeclarationAccepted,
                LiabilityDeclarationAcceptedOnUtc = DateTime.UtcNow,
                LiabilityDeclarationIp = ip,
                LiabilityDeclarationTextSnapshot = declarationTextSnapshot
            };

            context.TournamentJoinPayments.Add(order);
            await context.SaveChangesAsync();

            // Built from Site:BaseUrl, not from the Host header of the request.
            var req = httpContextAccessor.HttpContext?.Request;
            var successUrl = site.Absolute($"/City/PaymentSuccess?orderId={order.Id}", req);
            var cancelUrl = site.Absolute($"/City/PaymentCancel?orderId={order.Id}", req);

            var sessionOptions = new SessionCreateOptions
            {
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,

                // Abandoned checkouts close on their own (Stripe requires at least 30 minutes).
                ExpiresAt = DateTime.UtcNow.AddMinutes(31),

                PaymentMethodTypes = new List<string> { "card" },

                LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = settings.Currency,
                        UnitAmount = amount,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"Tournament participation fee (#{tournamentId})",
                        }
                    }
                }
            },

                Metadata = new Dictionary<string, string>
                {
                    ["orderId"] = order.Id.ToString(),
                    ["tournamentId"] = tournamentId.ToString(),
                    ["userId"] = userId,
                    ["teamId"] = teamId?.ToString() ?? "",
                    ["culture"] = order.Culture
                }
            };

            var sessionService = new SessionService();
            var session = await sessionService.CreateAsync(sessionOptions);

            order.StripeSessionId = session.Id;
            order.StripePaymentIntentId = string.IsNullOrWhiteSpace(session.PaymentIntentId) ? null : session.PaymentIntentId;
            await context.SaveChangesAsync();

            return session.Url;
        }

        public async Task<int?> ReusePaidOrderAsync(int tournamentId, string userId, int teamId)
        {
            // The coach left the tournament earlier and the fee was not returned: it still pays for the place.
            var order = await context.TournamentJoinPayments
                .Where(o => o.TournamentId == tournamentId && o.UserId == userId && o.Status == "Paid")
                .OrderByDescending(o => o.PaidOnUtc)
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return null;
            }

            if (order.TeamId != teamId)
            {
                order.TeamId = teamId;
                await context.SaveChangesAsync();
            }

            return order.Id;
        }

        public async Task ExpireOpenCheckoutsAsync(int tournamentId)
        {
            var pendingOrders = await context.TournamentJoinPayments
                .Where(o => o.TournamentId == tournamentId && o.Status == "Pending")
                .ToListAsync();

            var sessionService = new SessionService();

            foreach (var pending in pendingOrders)
            {
                if (!string.IsNullOrWhiteSpace(pending.StripeSessionId))
                {
                    try
                    {
                        await sessionService.ExpireAsync(pending.StripeSessionId);
                    }
                    catch (StripeException)
                    {
                        // Already expired or completed. If it was paid after all, the webhook finds an
                        // Expired order and refunds it automatically.
                    }
                }

                pending.Status = "Expired";
            }

            await context.SaveChangesAsync();
        }

        public async Task<CheckoutPaymentResult> ConfirmCheckoutPaymentAsync(CompletedCheckout checkout)
        {
            if (string.IsNullOrWhiteSpace(checkout.SessionId))
            {
                return new CheckoutPaymentResult(CheckoutPaymentOutcome.UnknownSession, reason: "no session id");
            }

            // Find the order by the Stripe session it was created with, never by the ids in the event:
            // one Stripe account can serve several copies of the site (local + live), each with its own order numbers.
            var order = await context.TournamentJoinPayments
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.StripeSessionId == checkout.SessionId);

            if (order == null)
            {
                return new CheckoutPaymentResult(CheckoutPaymentOutcome.UnknownSession, reason: "no order with this session");
            }

            if (!string.IsNullOrWhiteSpace(checkout.OrderId) && checkout.OrderId != order.Id.ToString())
            {
                return new CheckoutPaymentResult(CheckoutPaymentOutcome.Rejected, order, $"metadata orderId {checkout.OrderId} does not match");
            }

            if (checkout.PaymentStatus != "paid")
            {
                return new CheckoutPaymentResult(CheckoutPaymentOutcome.Rejected, order, $"payment status is {checkout.PaymentStatus}");
            }

            long expectedAmount = (long)Math.Round(order.Amount * 100m, MidpointRounding.AwayFromZero);
            if (checkout.AmountTotal != expectedAmount)
            {
                return new CheckoutPaymentResult(CheckoutPaymentOutcome.Rejected, order, $"amount {checkout.AmountTotal} instead of {expectedAmount}");
            }

            if (!string.Equals(checkout.Currency, order.Currency, StringComparison.OrdinalIgnoreCase))
            {
                return new CheckoutPaymentResult(CheckoutPaymentOutcome.Rejected, order, $"currency {checkout.Currency} instead of {order.Currency}");
            }

            // Only an order that is still waiting for its money can become Paid. Paid / refunded orders are
            // left alone, so an event that Stripe sends again can never bring a refunded team back.
            if (order.Status != "Pending" && order.Status != "Expired")
            {
                return new CheckoutPaymentResult(CheckoutPaymentOutcome.AlreadyProcessed, order, $"status {order.Status}");
            }

            var marked = await TryMarkPaidAsync(order.Id, order.Status, checkout.PaymentIntentId);
            if (!marked)
            {
                // Another delivery of the same event changed the order first.
                return new CheckoutPaymentResult(CheckoutPaymentOutcome.AlreadyProcessed, order, "confirmed by a parallel request");
            }

            var outcome = order.Status == "Pending" ? CheckoutPaymentOutcome.Join : CheckoutPaymentOutcome.Refund;

            order.Status = "Paid";
            order.StripePaymentIntentId = string.IsNullOrWhiteSpace(checkout.PaymentIntentId) ? order.StripePaymentIntentId : checkout.PaymentIntentId;

            return new CheckoutPaymentResult(outcome, order);
        }

        /// <summary>
        /// Moves the order to Paid only if it still has the expected status. On SQL Server this is a single
        /// UPDATE with the status in the WHERE clause, so when two requests (or two server instances) handle
        /// the same payment at the same time, only one of them succeeds.
        /// </summary>
        private async Task<bool> TryMarkPaidAsync(int orderId, string expectedStatus, string? paymentIntentId)
        {
            var paidOn = DateTime.UtcNow;
            var intentId = string.IsNullOrWhiteSpace(paymentIntentId) ? null : paymentIntentId;

            if (context.Database.IsRelational())
            {
                var rows = await context.Database.ExecuteSqlInterpolatedAsync(
                    $@"UPDATE TournamentJoinPayments
                       SET Status = 'Paid', PaidOnUtc = {paidOn}, StripePaymentIntentId = COALESCE({intentId}, StripePaymentIntentId)
                       WHERE Id = {orderId} AND Status = {expectedStatus}");

                return rows == 1;
            }

            // In-memory database (unit tests) has no SQL: same check, done in code.
            var order = await context.TournamentJoinPayments.FirstAsync(o => o.Id == orderId);
            if (order.Status != expectedStatus)
            {
                return false;
            }

            order.Status = "Paid";
            order.PaidOnUtc = paidOn;
            order.StripePaymentIntentId = intentId ?? order.StripePaymentIntentId;
            await context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> RefundTournamentJoinAsync(int orderId, decimal? amount = null, string? reason = null, bool ignoreTournamentStart = false)
        {
            var order = await context.TournamentJoinPayments
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) throw new ArgumentException("Order not found.");

            // "RefundFailed" can be retried (Stripe declined the previous attempt).
            if (order.Status != "Paid" && order.Status != "RefundFailed") throw new InvalidOperationException("Only PAID orders can be refunded.");
            if (!string.IsNullOrWhiteSpace(order.StripeRefundId) || order.Status == "Refunded")
                throw new InvalidOperationException("This order is already refunded (or refund is started).");

            if (string.IsNullOrWhiteSpace(order.StripePaymentIntentId))
                throw new InvalidOperationException("Missing Stripe PaymentIntentId for this order.");

            // Business rule: money is returned (and the team removed) only before the tournament starts.
            // The automatic refund after a failed registration skips this rule - the team never took part.
            if (!ignoreTournamentStart)
            {
                var startDate = await context.Tournaments
                    .Where(t => t.Id == order.TournamentId)
                    .Select(t => (DateTime?)t.StartDate)
                    .FirstOrDefaultAsync();

                if (startDate.HasValue && BgTime.Now >= startDate.Value)
                {
                    throw new InvalidOperationException("Refunds are possible only before the tournament starts.");
                }
            }

            long? amountCents = null;
            if (amount.HasValue)
            {
                if (amount.Value <= 0) throw new ArgumentException("Refund amount must be > 0.");
                if (amount.Value > order.Amount) throw new ArgumentException("Refund amount cannot exceed paid amount.");

                amountCents = (long)Math.Round(amount.Value * 100m, MidpointRounding.AwayFromZero);
            }

            var previousStatus = order.Status;
            order.Status = "RefundPending";
            order.RefundReason = reason;
            await context.SaveChangesAsync();

            var refundService = new RefundService();

            var options = new RefundCreateOptions
            {
                PaymentIntent = order.StripePaymentIntentId,
                Amount = amountCents,
                Reason = string.IsNullOrWhiteSpace(reason) ? null : reason,
                Metadata = new Dictionary<string, string>
                {
                    ["orderId"] = order.Id.ToString(),
                    ["tournamentId"] = order.TournamentId.ToString(),
                    ["userId"] = order.UserId
                }
            };

            var requestOptions = new RequestOptions
            {
                // Same key within a minute = protection against double clicks; a later retry gets a new key.
                IdempotencyKey = $"refund_order_{order.Id}_{amountCents?.ToString() ?? "full"}_{DateTime.UtcNow:yyyyMMddHHmm}"
            };

            Refund refund;
            try
            {
                refund = await refundService.CreateAsync(options, requestOptions);
            }
            catch
            {
                // Stripe rejected the request: don't leave the order stuck in "RefundPending".
                order.Status = previousStatus;
                await context.SaveChangesAsync();
                throw;
            }

            if (refund.Status == "succeeded")
            {
                order.StripeRefundId = refund.Id;
                order.Status = "Refunded";
                order.RefundedOnUtc = DateTime.UtcNow;
                order.RefundAmount = amount ?? order.Amount;
            }
            else if (refund.Status == "failed" || refund.Status == "canceled")
            {
                // Keep no refund id, so the admin can try again.
                order.StripeRefundId = null;
                order.Status = "RefundFailed";
            }
            else
            {
                order.StripeRefundId = refund.Id;
                order.Status = "RefundPending";
            }

            await context.SaveChangesAsync();
            return order.Status != "RefundFailed";
        }

        public async Task<AdminPaymentViewModel?> GetPaymentAsync(int orderId)
        {
            return await context.TournamentJoinPayments
                .AsNoTracking()
                .Where(p => p.Id == orderId)
                .Select(p => new AdminPaymentViewModel
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    TournamentId = p.TournamentId,
                    TeamId = p.TeamId,
                    Amount = p.Amount,
                    Status = p.Status,
                    PaidOnUtc = p.PaidOnUtc,
                    DeletedTournamentCity = p.TournamentCity,
                    // Paid, but the team is no longer in the tournament: it left and may ask for its money.
                    TeamLeft = p.Status == "Paid" && p.TournamentId != null && p.TeamId != null &&
                        !context.TournamentsTeams.Any(tt => tt.TournamentId == p.TournamentId && tt.TeamId == p.TeamId),
                    TournamentStartDate = context.Tournaments
                        .Where(t => t.Id == p.TournamentId)
                        .Select(t => (DateTime?)t.StartDate)
                        .FirstOrDefault() ?? p.TournamentStartDate
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ICollection<AdminPaymentViewModel>> GetAllTournamentJoinPaymentsAsync()
        {
            return await context.TournamentJoinPayments
                .AsNoTracking()
                .OrderByDescending(p => p.PaidOnUtc)
                .Select(p => new AdminPaymentViewModel
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    TournamentId = p.TournamentId,
                    TeamId = p.TeamId,
                    Amount = p.Amount,
                    Status = p.Status,
                    PaidOnUtc = p.PaidOnUtc,
                    DeletedTournamentCity = p.TournamentCity,
                    // Paid, but the team is no longer in the tournament: it left and may ask for its money.
                    TeamLeft = p.Status == "Paid" && p.TournamentId != null && p.TeamId != null &&
                        !context.TournamentsTeams.Any(tt => tt.TournamentId == p.TournamentId && tt.TeamId == p.TeamId),
                    TournamentStartDate = context.Tournaments
                        .Where(t => t.Id == p.TournamentId)
                        .Select(t => (DateTime?)t.StartDate)
                        .FirstOrDefault() ?? p.TournamentStartDate
                })
                .ToListAsync();
        }
    }

}

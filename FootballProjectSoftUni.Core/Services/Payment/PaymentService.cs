using FootballProjectSoftUni.Core.Contracts.Payment;
using FootballProjectSoftUni.Core.Models.Settings;
using FootballProjectSoftUni.Infrastructure.Data;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stripe.Checkout;

namespace FootballProjectSoftUni.Core.Services.Payment
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext context;
        private readonly StripeSettings settings;
        private readonly IHttpContextAccessor httpContextAccessor;

        public PaymentService(
            ApplicationDbContext context,
            IOptions<StripeSettings> options,
            IHttpContextAccessor httpContextAccessor)
        {
            this.context = context;
            this.settings = options.Value;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> CreateTournamentJoinCheckoutAsync(int tournamentId, string userId, int? teamId)
        {
            // 1) Load tournament fee from DB (source of truth)
            var tournament = await context.Tournaments.FirstOrDefaultAsync(t => t.Id == tournamentId);
            if (tournament == null)
            {
                throw new ArgumentException("Tournament not found.");
            }

            var fee = tournament.ParticipationFee; // decimal
            if (fee <= 0)
            {
                throw new InvalidOperationException("Participation fee is not configured.");
            }

            long amount = (long)Math.Round(fee * 100m, MidpointRounding.AwayFromZero);
            if (amount <= 0) throw new InvalidOperationException("Invalid fee amount.");

            // 2) Create payment order first (Pending)
            var order = new FootballProjectSoftUni.Infrastructure.Data.Models.TournamentJoinPayment
            {
                TournamentId = tournamentId,
                UserId = userId,
                TeamId = teamId,
                Amount = fee,
                Currency = settings.Currency,
                Status = "Pending"
            };

            context.TournamentJoinPayments.Add(order);
            await context.SaveChangesAsync();

            // 3) Build absolute urls
            var req = httpContextAccessor.HttpContext!.Request;
            var baseUrl = $"{req.Scheme}://{req.Host}";

            // Success page can show "waiting confirmation"
            var successUrl = $"{baseUrl}/City/PaymentSuccess?orderId={order.Id}";
            var cancelUrl = $"{baseUrl}/City/PaymentCancel?orderId={order.Id}";

            // 4) Create Stripe checkout session
            var sessionOptions = new SessionCreateOptions
            {
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,

                // For cards, wallet, etc.
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
                    ["teamId"] = teamId?.ToString() ?? ""
                }
            };

            var sessionService = new SessionService();
            var session = await sessionService.CreateAsync(sessionOptions);

            // 5) Save session id (and later payment intent id)
            order.StripeSessionId = session.Id;
            // session.PaymentIntentId may be null depending on mode; often present after creation
            order.StripePaymentIntentId = session.PaymentIntentId ?? "";
            await context.SaveChangesAsync();

            return session.Url; // redirect user to Stripe
        }


    }

}

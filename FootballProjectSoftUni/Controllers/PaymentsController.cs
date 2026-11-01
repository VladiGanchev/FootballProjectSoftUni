using FootballProjectSoftUni.Core.Contracts.Coach;
using FootballProjectSoftUni.Core.Contracts.Payment;
using FootballProjectSoftUni.Core.Extensions;
using FootballProjectSoftUni.Core.Models.AdminPayments;
using FootballProjectSoftUni.Extensions;
using FootballProjectSoftUni.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Stripe;

namespace FootballProjectSoftUni.Controllers
{
    [Authorize]
    public class PaymentsController : Controller
    {
        private readonly IPaymentService paymentService;
        private readonly ILogger<PaymentsController> logger;
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly ICoachService coachService;

        public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger, IStringLocalizer<SharedResource> localizer, ICoachService coachService)
        {
            this.coachService = coachService;
            this.paymentService = paymentService;
            this.logger = logger;
            this.localizer = localizer;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsAdmin() == false)
            {
                return Unauthorized();
            }

            var payments = await paymentService.GetAllTournamentJoinPaymentsAsync();
            return View(payments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Refund(int orderId)
        {
            if (User.IsAdmin() == false)
            {
                return Unauthorized();
            }

            try
            {
                var payment = await paymentService.GetPaymentAsync(orderId);

                if (payment != null && payment.TournamentStartDate.HasValue && BgTime.Now >= payment.TournamentStartDate.Value)
                {
                    TempData["Error"] = localizer["RefundTournamentStarted"].Value;
                    return RedirectToAction("Index");
                }

                var refunded = await paymentService.RefundTournamentJoinAsync(orderId, amount: null, reason: "requested_by_customer");

                if (!refunded)
                {
                    TempData["Error"] = localizer["RefundStripeError"].Value;
                    return RedirectToAction("Index");
                }

                // The money is returned, so the team no longer takes part in the tournament.
                var removed = payment?.TournamentId != null && await coachService.LeaveTournamentAsync(payment.TournamentId.Value, payment.UserId);

                TempData["Success"] = removed
                    ? localizer["RefundDoneTeamRemoved"].Value
                    : localizer["RefundDone"].Value;
            }
            catch (Exception ex) when ((ex is InvalidOperationException || ex is ArgumentException)
                                       && localizer.KnownServiceMessage(ex.Message) is string message)
            {
                // Our own validation messages ("Only PAID orders can be refunded." ...) are meant for the admin;
                // any other error goes to the general catch below (logged, generic message).
                TempData["Error"] = message;
            }
            catch (StripeException ex)
            {
                logger.LogError(ex, "Stripe refund failed for order {OrderId}", orderId);
                TempData["Error"] = localizer["RefundStripeError"].Value;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Refund failed for order {OrderId}", orderId);
                TempData["Error"] = localizer["GenericErrorMessage"].Value;
            }

            return RedirectToAction("Index"); 
        }
    }
}

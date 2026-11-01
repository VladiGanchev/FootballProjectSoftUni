using FootballProjectSoftUni;
using FootballProjectSoftUni.Core.Contracts.Email;
using FootballProjectSoftUni.Core.Contracts.Notification;
using FootballProjectSoftUni.Core.Contracts.Payment;
using FootballProjectSoftUni.Core.Contracts.Coach;
using FootballProjectSoftUni.Core.Contracts.Team;
using FootballProjectSoftUni.Core.Models.Payment;
using FootballProjectSoftUni.Core.Models.Settings;
using FootballProjectSoftUni.Core.Services.Email;
using FootballProjectSoftUni.Infrastructure.Data;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using System.Globalization;
using System.Net;

[AllowAnonymous]
[IgnoreAntiforgeryToken]
[Route("stripe/webhook")]
[ApiController]
public class StripeWebhookController : ControllerBase
{
    private readonly ApplicationDbContext context;
    private readonly StripeSettings settings;
    private readonly ITeamService teamService;
    private INotificationService notificationService;
    private readonly IEmailQueue emailQueue;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly IWebHostEnvironment env;
    private readonly IPaymentService paymentService;
    private readonly ICoachService coachService;

    public StripeWebhookController(
        ApplicationDbContext context,
        IOptions<StripeSettings> options,
        ITeamService teamService,
        INotificationService notificationService,
        IEmailQueue emailQueue,
        IStringLocalizer<SharedResource> localizer,
        IWebHostEnvironment env,
        IPaymentService paymentService,
        ICoachService coachService)
    {
        this.context = context;
        this.settings = options.Value;
        this.teamService = teamService;
        this.notificationService = notificationService;
        this.emailQueue = emailQueue;
        this.localizer = localizer;
        this.env = env;
        this.paymentService = paymentService;
        this.coachService = coachService;
    }

    // Stripe sends several events at the same moment; serialize writes to the log file
    // so concurrent requests don't fail with "file is being used by another process".
    private static readonly SemaphoreSlim logLock = new SemaphoreSlim(1, 1);
    private const long MaxLogBytes = 1024 * 1024;
    private const int MaxLogLineLength = 500;

    private async Task LogAsync(string line)
    {
        await logLock.WaitAsync();
        try
        {
            var logPath = Path.Combine(env.ContentRootPath, "stripe-webhook-log.txt");

            // Keep the log bounded: anyone on the internet can POST to /stripe/webhook.
            // When it reaches 1 MB the file is rotated to .old (overwriting the previous one), so it never exceeds ~2 MB.
            var info = new FileInfo(logPath);
            if (info.Exists && info.Length > MaxLogBytes)
            {
                System.IO.File.Move(logPath, logPath + ".old", overwrite: true);
            }

            if (line.Length > MaxLogLineLength)
            {
                line = line.Substring(0, MaxLogLineLength) + "...";
            }

            await System.IO.File.AppendAllTextAsync(logPath, $"[{DateTime.UtcNow:u}] {line}{Environment.NewLine}");
        }
        catch (Exception ex)
        {
            // Logging must never break webhook processing.
            Console.WriteLine("Stripe webhook log error: " + ex.Message);
        }
        finally
        {
            logLock.Release();
        }
    }

    /// <summary>
    /// What the refund was for: the tournament, or - when it was deleted - the city and date saved with the payment.
    /// </summary>
    private async Task<string> RefundEmailBodyAsync(TournamentJoinPayment order, decimal amount)
    {
        var tournament = order.TournamentId.HasValue
            ? await context.Tournaments
                .Where(t => t.Id == order.TournamentId)
                .Select(t => new
                {
                    City = t.TournamentCities.Select(tc => tc.City.Name).FirstOrDefault(),
                    StartDate = (DateTime?)t.StartDate
                })
                .FirstOrDefaultAsync()
            : null;

        var city = tournament?.City ?? order.TournamentCity;
        var start = tournament?.StartDate ?? order.TournamentStartDate;

        return city != null && start.HasValue
            ? string.Format(localizer["RefundSuccessBody"].Value, Money(amount), Html(city), EmailDate(start.Value))
            : string.Format(localizer["RefundSuccessBodyShort"].Value, Money(amount));
    }

    // E-mails are HTML: names typed by users go in as plain text.
    private static string Html(string? text) => WebUtility.HtmlEncode(text ?? string.Empty);

    private static string Money(decimal amount) => amount.ToString("0.00", CultureInfo.CurrentCulture);

    private static string EmailDate(DateTime date) => date.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);

    private async Task RefundAndNotifyAsync(TournamentJoinPayment order, string reason)
    {
        await LogAsync(reason);

        try
        {
            await paymentService.RefundTournamentJoinAsync(order.Id, null, "requested_by_customer", ignoreTournamentStart: true);
            await LogAsync($"AUTO-REFUND started for order {order.Id}");
        }
        catch (Exception refundEx)
        {
            await LogAsync($"AUTO-REFUND FAILED for order {order.Id}: {refundEx.Message}");
        }

        try
        {
            await notificationService.CreateNotificationForUserAsync(
                order.UserId,
                localizer["JoinFailedRefundNotification"].Value);
        }
        catch (Exception notifyEx)
        {
            await LogAsync($"NOTIFY FAILED for order {order.Id}: {notifyEx.Message}");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Post()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        Event stripeEvent;
        try
        {
            var signatureHeader = Request.Headers["Stripe-Signature"];
            stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, settings.WebhookSecret);

            await LogAsync($"Event type: {stripeEvent.Type}");
        }
        catch (Exception ex)
        {
            await LogAsync($"REJECTED (invalid request/signature): {ex.GetType().Name}");

            Console.WriteLine("Stripe webhook error: " + ex.Message);
            return BadRequest();
        }

        if (stripeEvent.Type == "checkout.session.completed" || stripeEvent.Type == "checkout.session.async_payment_succeeded")
        {
            var session = stripeEvent.Data.Object as Session;
            if (session == null) return Ok();

            string? metadataOrderId = null;
            session.Metadata?.TryGetValue("orderId", out metadataOrderId);

            // The order is matched by its Stripe session, and amount, currency and status are checked;
            // only an order that is still waiting for this payment becomes Paid (exactly once).
            var result = await paymentService.ConfirmCheckoutPaymentAsync(new CompletedCheckout
            {
                SessionId = session.Id,
                OrderId = metadataOrderId,
                PaymentStatus = session.PaymentStatus,
                AmountTotal = session.AmountTotal,
                Currency = session.Currency,
                PaymentIntentId = session.PaymentIntentId
            });

            if (result.Outcome == CheckoutPaymentOutcome.UnknownSession)
            {
                // Normal when one Stripe account serves several copies of the site: not our payment.
                await LogAsync($"IGNORED session {session.Id}: {result.Reason}");
                return Ok();
            }

            if (result.Outcome == CheckoutPaymentOutcome.Rejected)
            {
                await LogAsync($"REJECTED session {session.Id} for order {result.Order?.Id}: {result.Reason}");
                return Ok();
            }

            if (result.Outcome == CheckoutPaymentOutcome.AlreadyProcessed)
            {
                await LogAsync($"ALREADY PROCESSED order {result.Order?.Id}: {result.Reason}");
                return Ok();
            }

            var order = result.Order!;

            var culture = order.Culture ?? "bg";
            var cultureInfo = new CultureInfo(culture);
            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;

            if (result.Outcome == CheckoutPaymentOutcome.Refund)
            {
                // Paid for an order that was no longer active (a newer checkout replaced it): return the money.
                await RefundAndNotifyAsync(order, $"PAID INACTIVE order {order.Id}");
                return Ok();
            }

            if (!order.TeamId.HasValue)
            {
                return Ok();
            }

            if (!order.TournamentId.HasValue)
            {
                // The tournament was deleted while the user was paying: return the money.
                await RefundAndNotifyAsync(order, $"TOURNAMENT DELETED for order {order.Id}");
                return Ok();
            }

            var cityId = await context.TournamentsCities
                .Where(x => x.TournamentId == order.TournamentId)
                .Select(x => x.City.Id)
                .FirstOrDefaultAsync();

            try
            {
                await teamService.FinalizeJoinAsync(order.TournamentId.Value, order.UserId, order.TeamId.Value, cityId);
            }
            catch (Exception joinEx)
            {
                // The money was taken but the team could not be registered (tournament full, rules
                // violated, team removed...). Never keep the money: refund it and notify the user.
                await RefundAndNotifyAsync(order, $"JOIN FAILED for order {order.Id}: {joinEx.Message}");
                return Ok();
            }

            var tournamentInfo = await context.Tournaments
                .Where(t => t.Id == order.TournamentId)
                .Select(t => new
                {
                    t.StartDate,
                    City = t.TournamentCities.Select(tc => tc.City.Name).FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            var cityName = tournamentInfo?.City;


            var teamName = await context.Teams
                .Where(t => t.Id == order.TeamId.Value) 
                .Select(t => t.Name)
                .FirstOrDefaultAsync();

            var fee = await context.Tournaments.Where(x => x.Id == order.TournamentId).Select(x => x.ParticipationFee).FirstOrDefaultAsync();

            //var message =$"✅ Плащането на сумата от {fee} € е успешно! Отборът {teamName} е регистриран за турнир в град {cityName}.";
            var message = string.Format(
                localizer["PaymentSuccessNotification"].Value,
                fee,
                teamName,
                cityName);

            await notificationService.CreateNotificationForUserAsync(order.UserId, message);

            var userEmail = await context.Users
                .Where(u => u.Id == order.UserId)
                .Select(u => u.Email)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(userEmail))
            {
                var subject = localizer["PaymentSuccessSubject"].Value;
                var body = string.Format(
                    localizer["PaymentSuccessBody"].Value,
                    Html(teamName),
                    Money(order.Amount),
                    Html(cityName),
                    tournamentInfo != null ? EmailDate(tournamentInfo.StartDate) : string.Empty);

                try
                {
                    emailQueue.Enqueue(userEmail, subject, body);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("EMAIL ERROR: " + ex.ToString());
                }
            }
        }

        if (stripeEvent.Type == "checkout.session.expired")
        {
            var expiredSession = stripeEvent.Data.Object as Session;
            if (expiredSession != null)
            {
                var expiredOrder = await context.TournamentJoinPayments
                    .FirstOrDefaultAsync(o => o.StripeSessionId == expiredSession.Id && o.Status == "Pending");

                if (expiredOrder != null)
                {
                    expiredOrder.Status = "Expired";
                    await context.SaveChangesAsync();
                }
            }

            return Ok();
        }

        if (stripeEvent.Type == "charge.refunded")
        {
            var charge = stripeEvent.Data.Object as Charge;
            if (charge == null) return Ok();

            var piId = charge.PaymentIntentId;
            if (string.IsNullOrWhiteSpace(piId)) return Ok();

            var order = await context.TournamentJoinPayments
                .FirstOrDefaultAsync(o => o.StripePaymentIntentId == piId);

            if (order == null) return Ok();

            var culture = order.Culture ?? "bg";
            var cultureInfo = new CultureInfo(culture);
            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;

            // Partial refund (e.g. made from the Stripe Dashboard): record the amount, the order stays paid.
            if (!charge.Refunded)
            {
                order.RefundAmount = charge.AmountRefunded / 100m;
                await context.SaveChangesAsync();
                return Ok();
            }

            var wasAlreadyRefunded = order.Status == "Refunded";

            order.Status = "Refunded";
            order.RefundedOnUtc ??= DateTime.UtcNow;
            order.RefundAmount = charge.AmountRefunded / 100m;

            await context.SaveChangesAsync();

            // Full refund before the tournament starts: the team no longer takes part.
            // (Admin refunds from the site already removed it; LeaveTournamentAsync then simply returns false.)
            if (!wasAlreadyRefunded)
            {
                var hasOtherPaidOrder = await context.TournamentJoinPayments.AnyAsync(o =>
                    o.Id != order.Id && o.UserId == order.UserId && o.TournamentId == order.TournamentId && o.Status == "Paid");

                if (!hasOtherPaidOrder && order.TournamentId.HasValue)
                {
                    try
                    {
                        await coachService.LeaveTournamentAsync(order.TournamentId.Value, order.UserId);
                    }
                    catch (Exception leaveEx)
                    {
                        await LogAsync($"REMOVE TEAM after refund failed for order {order.Id}: {leaveEx.Message}");
                    }
                }
            }
            var userEmail = await context.Users
              .Where(u => u.Id == order.UserId)
              .Select(u => u.Email)
              .FirstOrDefaultAsync(); 
            if (!string.IsNullOrWhiteSpace(userEmail))
            {
                var subject = localizer["RefundSuccessSubject"].Value;
                var body = await RefundEmailBodyAsync(order, charge.AmountRefunded / 100m);

                try
                {
                    emailQueue.Enqueue(userEmail, subject, body);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("EMAIL ERROR: " + ex.ToString());
                }
            }
        }

        return Ok();
    }
}

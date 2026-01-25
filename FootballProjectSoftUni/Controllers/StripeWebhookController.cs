using FootballProjectSoftUni.Core.Contracts.Email;
using FootballProjectSoftUni.Core.Contracts.Notification;
using FootballProjectSoftUni.Core.Contracts.Team;
using FootballProjectSoftUni.Core.Models.Settings;
using FootballProjectSoftUni.Core.Services.Email;
using FootballProjectSoftUni.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

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
    private IEmailService emailService;

    public StripeWebhookController(
        ApplicationDbContext context,
        IOptions<StripeSettings> options,
        ITeamService teamService,
        INotificationService notificationService,
        IEmailService emailService)
    {
        this.context = context;
        this.settings = options.Value;
        this.teamService = teamService;
        this.notificationService = notificationService;
        this.emailService = emailService;
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
        }
        catch (Exception ex)
        {
            Console.WriteLine("Stripe webhook error: " + ex.Message);
            return BadRequest();
        }

        if (stripeEvent.Type == "checkout.session.completed")
        {
            var session = stripeEvent.Data.Object as Session;
            if (session == null) return Ok();

            if (!session.Metadata.TryGetValue("orderId", out var orderIdStr))
                return Ok();

            if (!int.TryParse(orderIdStr, out var orderId))
                return Ok();

            var order = await context.TournamentJoinPayments.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return Ok();

            if (order.Status == "Paid") return Ok();

            order.Status = "Paid";
            order.PaidOnUtc = DateTime.UtcNow;
            order.StripePaymentIntentId = session.PaymentIntentId ?? order.StripePaymentIntentId;

            await context.SaveChangesAsync();

            if (!order.TeamId.HasValue)
            {
                return Ok();
            }

            await teamService.FinalizeJoinAsync(order.TournamentId, order.UserId, order.TeamId.Value);

            var cityName = await context.TournamentsCities
                .Where(x => x.TournamentId == order.TournamentId)
                .Select(x => x.City.Name)
                .FirstOrDefaultAsync();

            var teamName = await context.Teams
                .Where(t => t.Id == order.TeamId.Value) 
                .Select(t => t.Name)
                .FirstOrDefaultAsync();

            var fee = await context.Tournaments.Where(x => x.Id == order.TournamentId).Select(x => x.ParticipationFee).FirstOrDefaultAsync();

            var message =$"✅ Плащането на сумата от {fee} € е успешно! Отборът {teamName} е регистриран за турнир в град {cityName}.";

            await notificationService.CreateNotificationForUserAsync(order.UserId, message);

            var userEmail = await context.Users
                .Where(u => u.Id == order.UserId)
                .Select(u => u.Email)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrWhiteSpace(userEmail))
            {
                var subject = "Успешно плащане ⚽";
                var body = $"<p>Плащането е успешно. Отборът <b>{teamName}</b> е регистриран!</p>";

                try
                {
                    await emailService.SendAsync(userEmail, subject, body);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("EMAIL ERROR: " + ex.ToString());
                }
            }
        }

        return Ok();
    }
}

using FootballProjectSoftUni.Core.Contracts.Email;
using FootballProjectSoftUni.Core.Contracts.Notification;
using FootballProjectSoftUni.Core.Contracts.Tournament;
using FootballProjectSoftUni.Core.Models.Tournament;
using FootballProjectSoftUni.Core.Services.Tournament;
using FootballProjectSoftUni.Infrastructure.Data;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Net;
using static FootballProjectSoftUni.Core.Constants.RoleConstants;

namespace FootballProjectSoftUni.Services
{
    /// <summary>
    /// Runs every few minutes (Hangfire):
    /// - 24 hours before a tournament with too few teams, warns the organiser (who can still move the date);
    /// - when a tournament starts: with too few teams it is cancelled and everyone gets their money back,
    ///   otherwise teams without an opponent go through to the next round.
    /// </summary>
    public class TournamentStartJob
    {
        private readonly ITournamentStartService startService;
        private readonly TournamentDeletionFollowUp followUp;
        private readonly INotificationService notificationService;
        private readonly IEmailQueue emailQueue;
        private readonly ApplicationDbContext context;
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly ILogger<TournamentStartJob> logger;

        public TournamentStartJob(
            ITournamentStartService startService,
            TournamentDeletionFollowUp followUp,
            INotificationService notificationService,
            IEmailQueue emailQueue,
            ApplicationDbContext context,
            IStringLocalizer<SharedResource> localizer,
            ILogger<TournamentStartJob> logger)
        {
            this.startService = startService;
            this.followUp = followUp;
            this.notificationService = notificationService;
            this.emailQueue = emailQueue;
            this.context = context;
            this.localizer = localizer;
            this.logger = logger;
        }

        [DisableConcurrentExecution(timeoutInSeconds: 300)]
        public async Task RunAsync()
        {
            // Messages to the organiser are in Bulgarian, the main language of the site.
            UseBulgarian();

            foreach (var warning in await startService.TakeLowTeamsWarningsAsync())
            {
                var message = localizer["AdminLowTeamsWarning",
                    warning.City ?? "-", Date(warning.StartDate), warning.Teams, TournamentStartService.MinimumTeams].Value;

                await NotifyAdminsAsync(message, localizer["AdminLowTeamsWarningSubject"].Value);
            }

            foreach (var result in await startService.StartDueTournamentsAsync())
            {
                if (result.Outcome == TournamentStartOutcome.Started)
                {
                    logger.LogInformation("Tournament {TournamentId} started with {Teams} teams.", result.TournamentId, result.Teams);
                    continue;
                }

                // Teams whose money was returned are told in their own language (also after a partial failure).
                await followUp.RunAsync(result.Cancellation!, notEnoughTeams: true);
                UseBulgarian();

                if (result.Outcome == TournamentStartOutcome.Canceled)
                {
                    var message = localizer["AdminAutoCanceled",
                        result.City ?? "-", Date(result.StartDate), result.Teams, result.Cancellation!.Refunded.Count].Value;

                    await NotifyAdminsAsync(message, localizer["AdminAutoCanceledSubject"].Value);
                }
                else
                {
                    // Tried again every few minutes; tell the organiser only once per set of failed teams.
                    var message = localizer["AdminAutoCancelFailed",
                        result.City ?? "-", Date(result.StartDate), string.Join(", ", result.Cancellation!.FailedTeams)].Value;

                    logger.LogWarning("Automatic cancellation of tournament {TournamentId} failed: {Message}", result.TournamentId, message);
                    await NotifyAdminsAsync(message, localizer["AdminAutoCanceledSubject"].Value, onlyOnce: true);
                }
            }
        }

        private async Task NotifyAdminsAsync(string message, string emailSubject, bool onlyOnce = false)
        {
            var admins = await context.Users
                .Where(u => context.UserRoles.Any(ur => ur.UserId == u.Id &&
                                                        context.Roles.Any(r => r.Id == ur.RoleId && r.Name == AdminRole)))
                .Select(u => new { u.Id, u.Email })
                .ToListAsync();

            foreach (var admin in admins)
            {
                if (onlyOnce && await context.Notifications.AnyAsync(n => n.UserId == admin.Id && n.Message == message))
                {
                    continue;
                }

                try
                {
                    await notificationService.CreateNotificationForUserAsync(admin.Id, message);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Notification for admin {UserId} failed", admin.Id);
                }

                if (string.IsNullOrWhiteSpace(admin.Email))
                {
                    continue;
                }

                try
                {
                    emailQueue.Enqueue(admin.Email, emailSubject, $"<p>{WebUtility.HtmlEncode(message)}</p><p>Tournament Galaxy</p>");
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "E-mail to admin {UserId} failed", admin.Id);
                }
            }
        }

        private static void UseBulgarian()
        {
            var bg = new CultureInfo("bg");
            CultureInfo.CurrentCulture = bg;
            CultureInfo.CurrentUICulture = bg;
        }

        private static string Date(DateTime date) => date.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
    }
}

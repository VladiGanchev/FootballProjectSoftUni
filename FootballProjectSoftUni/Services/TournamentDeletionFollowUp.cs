using FootballProjectSoftUni.Core.Contracts.Email;
using FootballProjectSoftUni.Core.Contracts.Notification;
using FootballProjectSoftUni.Core.Models.Tournament;
using FootballProjectSoftUni.Extensions;
using FootballProjectSoftUni.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Net;

namespace FootballProjectSoftUni.Services
{
    /// <summary>
    /// After a tournament or city deletion: tells the affected users (in their own language)
    /// and removes the pictures and the galleries of what was deleted.
    /// </summary>
    public class TournamentDeletionFollowUp
    {
        private readonly INotificationService notificationService;
        private readonly IEmailQueue emailQueue;
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly ApplicationDbContext context;
        private readonly IWebHostEnvironment env;
        private readonly CoverPictureUpload pictures;
        private readonly ILogger<TournamentDeletionFollowUp> logger;

        public TournamentDeletionFollowUp(
            INotificationService notificationService,
            IEmailQueue emailQueue,
            IStringLocalizer<SharedResource> localizer,
            ApplicationDbContext context,
            IWebHostEnvironment env,
            CoverPictureUpload pictures,
            ILogger<TournamentDeletionFollowUp> logger)
        {
            this.notificationService = notificationService;
            this.emailQueue = emailQueue;
            this.localizer = localizer;
            this.context = context;
            this.env = env;
            this.pictures = pictures;
            this.logger = logger;
        }

        /// <param name="notEnoughTeams">Cancelled automatically at the start because too few teams signed up
        /// (the messages say why).</param>
        public async Task RunAsync(TournamentDeletionResult result, bool notEnoughTeams = false)
        {
            // Refunded teams are told even when the deletion stopped: their money is back and they are out.
            foreach (var team in result.Refunded)
            {
                await NotifyRefundedAsync(team, notEnoughTeams);
            }

            var canceledKey = notEnoughTeams ? "TournamentCanceledLowTeamsNotification" : "TournamentCanceledNotification";

            foreach (var participant in result.OtherParticipants)
            {
                await NotifyAsync(participant.UserId, CultureScope.Run("bg", () =>
                    string.Format(localizer[canceledKey].Value, participant.City, Date(participant.StartDate))));
            }

            foreach (var tournamentId in result.DeletedTournamentIds)
            {
                RemoveGallery(tournamentId);
            }

            foreach (var url in result.DeletedPictureUrls)
            {
                pictures.Delete(url);
            }
        }

        private async Task NotifyRefundedAsync(RefundedTeam team, bool notEnoughTeams)
        {
            var culture = team.Culture == "en" ? "en" : "bg";

            var (message, subject, body) = CultureScope.Run(culture, () =>
            {
                var amount = team.Amount.ToString("0.00", CultureInfo.CurrentCulture);
                var date = Date(team.StartDate);

                return (
                    string.Format(localizer[notEnoughTeams ? "TournamentCanceledLowTeamsRefundNotification" : "TournamentCanceledRefundNotification"].Value, team.City, date, amount),
                    localizer["TournamentCanceledSubject"].Value,
                    string.Format(localizer[notEnoughTeams ? "TournamentCanceledLowTeamsEmailBody" : "TournamentCanceledEmailBody"].Value, WebUtility.HtmlEncode(team.City ?? string.Empty), date, amount));
            });

            await NotifyAsync(team.UserId, message);

            var email = await context.Users
                .Where(u => u.Id == team.UserId)
                .Select(u => u.Email)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(email))
            {
                return;
            }

            try
            {
                emailQueue.Enqueue(email, subject, body);
            }
            catch (Exception ex)
            {
                // The refund is done; a failed email must not undo or block anything.
                logger.LogWarning(ex, "Tournament cancellation email to user {UserId} failed", team.UserId);
            }
        }

        private async Task NotifyAsync(string userId, string message)
        {
            try
            {
                await notificationService.CreateNotificationForUserAsync(userId, message);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Tournament cancellation notification for user {UserId} failed", userId);
            }
        }

        private void RemoveGallery(int tournamentId)
        {
            var folder = Path.Combine(env.WebRootPath, "images", "tournaments", tournamentId.ToString());

            try
            {
                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, recursive: true);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Gallery of deleted tournament {TournamentId} could not be removed", tournamentId);
            }
        }

        private static string Date(DateTime date) => date.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
    }
}

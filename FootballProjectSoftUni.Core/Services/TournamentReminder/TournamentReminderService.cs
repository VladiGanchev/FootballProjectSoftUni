using FootballProjectSoftUni.Core.Contracts.Email;
using FootballProjectSoftUni.Core.Contracts.Notification;
using FootballProjectSoftUni.Core.Contracts.TournamentReminder;
using FootballProjectSoftUni.Core.Extensions;
using FootballProjectSoftUni.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FootballProjectSoftUni.Core.Services.TournamentReminder
{
    public class TournamentReminderService : ITournamentReminderService
    {
        private readonly ApplicationDbContext data;
        private readonly INotificationService notificationService;
        private readonly IEmailQueue emailQueue;
        private readonly IReminderTexts texts;

        public TournamentReminderService(
            ApplicationDbContext data,
            INotificationService notificationService,
            IEmailQueue emailQueue,
            IReminderTexts texts)
        {
            this.data = data;
            this.notificationService = notificationService;
            this.emailQueue = emailQueue;
            this.texts = texts;
        }

        public async Task Send24HourRemindersAsync()
        {
            var now = BgTime.Now;
            var until = now.AddHours(24);

            // Less than 24 hours to the start and no reminder yet. (Not "exactly 23-25 hours": when the site
            // was asleep at that moment, the reminder still goes out before the start instead of never.)
            var due = await data.Tournaments
                .Where(t => !t.ReminderSent && t.StartDate > now && t.StartDate <= until)
                .Select(t => new
                {
                    t.Id,
                    t.StartDate,
                    City = t.TournamentCities
                        .Select(tc => tc.City.Name)
                        .FirstOrDefault()
                })
                .ToListAsync();

            foreach (var tournament in due)
            {
                // Marked before anything is sent: whatever fails below, nobody gets this reminder twice.
                var entity = await data.Tournaments.FirstAsync(t => t.Id == tournament.Id);
                entity.ReminderSent = true;
                await data.SaveChangesAsync();

                var participantIds = await data.TournamentsParticipants
                    .Where(tp => tp.TournamentId == tournament.Id)
                    .Select(tp => tp.ParticipantId)
                    .Distinct()
                    .ToListAsync();

                var users = await data.Users
                    .Where(u => participantIds.Contains(u.Id))
                    .Select(u => new { u.Id, u.Email })
                    .ToListAsync();

                // The language a coach paid in; everybody else (e.g. the referee) gets Bulgarian.
                var englishUsers = await data.TournamentJoinPayments
                    .Where(p => p.TournamentId == tournament.Id && p.Status == "Paid" && p.Culture == "en")
                    .Select(p => p.UserId)
                    .Distinct()
                    .ToListAsync();

                foreach (var user in users)
                {
                    var text = texts.For(englishUsers.Contains(user.Id) ? "en" : "bg", tournament.City, tournament.StartDate);

                    try
                    {
                        await notificationService.CreateNotificationForUserAsync(user.Id, text.Notification);
                    }
                    catch (Exception)
                    {
                        // The e-mail below still goes out; one failure must not stop the other participants.
                    }

                    if (!string.IsNullOrWhiteSpace(user.Email))
                    {
                        emailQueue.Enqueue(user.Email, text.Subject, text.EmailHtml);
                    }
                }
            }
        }
    }
}

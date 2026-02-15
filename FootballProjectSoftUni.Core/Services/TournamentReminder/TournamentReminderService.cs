using FootballProjectSoftUni.Core.Contracts.Email;
using FootballProjectSoftUni.Core.Contracts.Notification;
using FootballProjectSoftUni.Core.Contracts.TournamentReminder;
using FootballProjectSoftUni.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FootballProjectSoftUni.Core.Services.TournamentReminder
{
    public class TournamentReminderService : ITournamentReminderService
    {
        private readonly ApplicationDbContext data;
        private readonly INotificationService notificationService;
        private readonly IEmailService emailService;

        public TournamentReminderService(
            ApplicationDbContext data,
            INotificationService notificationService,
            IEmailService emailService)
        {
            this.data = data;
            this.notificationService = notificationService;
            this.emailService = emailService;
        }

        public async Task Send24HourRemindersAsync()
        {
            var now = DateTime.Now;
            var from = now.AddHours(23);
            var to = now.AddHours(25);

            var tournaments = await data.Tournaments
                .Where(t => !t.ReminderSent
                         && t.StartDate >= from
                         && t.StartDate <= to)
                .Select(t => new
                {
                    t.Id,
                    t.StartDate,
                    CityName = t.TournamentCities
                        .Select(tc => tc.City.Name)
                        .FirstOrDefault()
                })
                .ToListAsync();

            foreach (var t in tournaments)
            {
                var cityName = string.IsNullOrWhiteSpace(t.CityName) ? "неизвестен град" : t.CityName;

                var userIds = await data.TournamentsParticipants
                    .Where(tp => tp.TournamentId == t.Id)
                    .Select(tp => tp.ParticipantId)
                    .Distinct()
                    .ToListAsync();

                if (userIds.Count == 0)
                {
                    var entityNoUsers = await data.Tournaments.FindAsync(t.Id);
                    if (entityNoUsers != null) entityNoUsers.ReminderSent = true;
                    continue;
                }

                var users = await data.Users
                    .Where(u => userIds.Contains(u.Id))
                    .Select(u => new { u.Id, u.Email })
                    .ToListAsync();

                var subject = "Напомняне за турнир";
                var message =
                    $"Напомняне: на {t.StartDate:dd.MM.yyyy} в {t.StartDate:HH:mm} " +
                    $"имаш участие в турнир в {cityName}.";

                foreach (var u in users)
                {
                    await notificationService.CreateNotificationForUserAsync(u.Id, message);

                    if (!string.IsNullOrWhiteSpace(u.Email))
                    {
                        await emailService.SendAsync(u.Email, subject, message);
                    }
                }

                var entity = await data.Tournaments.FindAsync(t.Id);
                if (entity != null)
                    entity.ReminderSent = true;
            }

            await data.SaveChangesAsync();
        }
    }
}

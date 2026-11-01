using FootballProjectSoftUni.Core.Contracts.Email;
using FootballProjectSoftUni.Core.Contracts.Notification;
using FootballProjectSoftUni.Core.Extensions;
using FootballProjectSoftUni.Core.Models.Notification;
using FootballProjectSoftUni.Infrastructure.Data;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;

namespace FootballProjectSoftUni.Core.Services.Notification
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext data;
        private readonly IEmailQueue emailQueue;

        public NotificationService(ApplicationDbContext _data, IEmailQueue _emailQueue)
        {
            data = _data;
            emailQueue = _emailQueue;
        }
        public async Task<IPagedList<NotificationViewModel>> AllNotificationsAsync(string userId, int pageNumber, int pageSize)
        {
            return await data.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedOn)
                .Include(n => n.ContactMessage)
                    .ThenInclude(cm => cm.User)
                .Select(n => new NotificationViewModel
                {
                    Id = n.Id,
                    Message = n.Message,
                    CreatedOn = n.CreatedOn,
                    IsRead = n.IsRead,
                    ContactMessageId = n.ContactMessageId,
                    FromName = n.ContactMessage != null
                        ? (n.ContactMessage.IsFromAdmin
                            ? "Админ"
                            : n.ContactMessage.User.FirstName + " " + n.ContactMessage.User.LastName)
                        : null
                })
                .ToPagedListAsync(Paging.ValidPage(pageNumber), pageSize);
        }

        public async Task CreateNotificationForCityCoachesAsync(int cityId, Func<string, AnnouncementText> textFor, string unsubscribeUrl)
        {
            var coachIds = await data.TournamentsParticipants
                .Where(tp => tp.Role == "Coach" &&
                             tp.Tournament.TournamentCities.Any(tc => tc.CityId == cityId))
                .Select(tp => tp.ParticipantId)
                .Distinct()
                .ToListAsync();

            if (!coachIds.Any())
                return;

            // The language of each coach's latest payment; Bulgarian for coaches who never paid.
            var payments = await data.TournamentJoinPayments
                .Where(p => coachIds.Contains(p.UserId))
                .Select(p => new { p.UserId, p.Culture, p.CreatedOnUtc })
                .ToListAsync();

            var englishUsers = payments
                .GroupBy(p => p.UserId)
                .Where(g => g.OrderByDescending(p => p.CreatedOnUtc).First().Culture == "en")
                .Select(g => g.Key)
                .ToHashSet();

            await AnnounceAsync(coachIds, englishUsers, textFor, unsubscribeUrl);
        }

        public async Task CreateNotificationForTournamentParticipantsAsync(int tournamentId, Func<string, AnnouncementText> textFor)
        {
            var participantIds = await data.TournamentsParticipants
                .Where(tp => tp.TournamentId == tournamentId)
                .Select(tp => tp.ParticipantId)
                .Distinct()
                .ToListAsync();

            if (!participantIds.Any())
                return;

            // As the reminder: coaches who paid for this tournament in English get English,
            // everybody else (e.g. the referee) Bulgarian.
            var englishUsers = (await data.TournamentJoinPayments
                .Where(p => p.TournamentId == tournamentId && p.Status == "Paid" && p.Culture == "en")
                .Select(p => p.UserId)
                .Distinct()
                .ToListAsync())
                .ToHashSet();

            await AnnounceAsync(participantIds, englishUsers, textFor);
        }

        /// <param name="unsubscribeUrl">Set for an advertisement (a new tournament): see the e-mail loop below.</param>
        private async Task AnnounceAsync(List<string> userIds, ISet<string> englishUsers, Func<string, AnnouncementText> textFor, string? unsubscribeUrl = null)
        {
            // Only existing accounts (a deleted user can't get a notification).
            var users = await data.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Email, u.NewTournamentEmails, u.SecurityStamp })
                .ToListAsync();

            var bulgarian = textFor("bg");
            var english = textFor("en");

            var announcements = users
                .Select(u => (u.Id, u.Email, u.NewTournamentEmails, u.SecurityStamp, Text: englishUsers.Contains(u.Id) ? english : bulgarian))
                .ToList();

            await data.Notifications.AddRangeAsync(announcements.Select(a => new FootballProjectSoftUni.Infrastructure.Data.Models.Notification
            {
                UserId = a.Id,
                Message = a.Text.Notification,
                CreatedOn = DateTime.UtcNow,
                IsRead = false
            }));
            await data.SaveChangesAsync();

            // Sent in the background: one failing address doesn't stop (or repeat) the others.
            foreach (var announcement in announcements.Where(a => !string.IsNullOrWhiteSpace(a.Email)))
            {
                var html = announcement.Text.EmailHtml;

                // An advertisement (a new tournament) is e-mailed only to those who asked for it, each time with
                // their own link that turns it off without signing in. The notification on the site goes to everybody.
                if (unsubscribeUrl != null)
                {
                    var key = UnsubscribeKey.For(announcement.Id, announcement.SecurityStamp);
                    if (!announcement.NewTournamentEmails || key == null)
                    {
                        continue;
                    }

                    var link = $"{unsubscribeUrl}?u={Uri.EscapeDataString(announcement.Id)}&k={Uri.EscapeDataString(key)}";
                    html = html.Replace(AnnouncementText.UnsubscribeLinkPlaceholder, WebUtility.HtmlEncode(link));
                }

                emailQueue.Enqueue(announcement.Email!, announcement.Text.Subject, html);
            }
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await data.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var unreadNotifications = await data.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (unreadNotifications.Any())
            {
                foreach (var notification in unreadNotifications)
                {
                    notification.IsRead = true;
                }

                await data.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id, string userId)
        {
            var notification = await data.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notification == null)
            {
                return;
            }

            data.Notifications.Remove(notification);
            await data.SaveChangesAsync();
        }

        public async Task CreateNotificationForUserAsync(string userId, string message)
        {
            var notification = new FootballProjectSoftUni.Infrastructure.Data.Models.Notification
            {
                UserId = userId,
                Message = message,
                CreatedOn = DateTime.UtcNow,
                IsRead = false,
                ContactMessageId = null
            };

            await data.Notifications.AddAsync(notification);
            await data.SaveChangesAsync();
        }

    }
}

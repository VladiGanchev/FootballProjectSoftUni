using FootballProjectSoftUni.Core.Contracts.Email;
using FootballProjectSoftUni.Core.Contracts.Notification;
using FootballProjectSoftUni.Core.Models.Notification;
using FootballProjectSoftUni.Infrastructure.Data;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;

namespace FootballProjectSoftUni.Core.Services.Notification
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext data;
        private readonly IEmailService emailService;

        public NotificationService(ApplicationDbContext _data, IEmailService _emailService)
        {
            data = _data;
            emailService = _emailService;
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
                .ToPagedListAsync(pageNumber, pageSize);
        }

        public async Task CreateNotificationForCityCoachesAsync(int cityId, string message)
        {
            var coachIds = await data.TournamentsParticipants
                .Where(tp => tp.Role == "Coach" &&
                             tp.Tournament.TournamentCities.Any(tc => tc.CityId == cityId))
                .Select(tp => tp.ParticipantId)
                .Distinct()
                .ToListAsync();

            if (!coachIds.Any())
                return;

            var userEmails = await data.Users
                .Where(u => coachIds.Contains(u.Id))
                .Select(u => u.Email)
                .ToListAsync();

            var notifications = coachIds.Select(coachId => new FootballProjectSoftUni.Infrastructure.Data.Models.Notification
            {
                UserId = coachId,
                Message = message,
                CreatedOn = DateTime.UtcNow,
                IsRead = false
            }).ToList();

            await data.Notifications.AddRangeAsync(notifications);
            await data.SaveChangesAsync();

            string subject = "Нов Турнир!";
            string content = message;

            foreach (var email in userEmails)
            {
                await emailService.SendAsync(email, subject, content);
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

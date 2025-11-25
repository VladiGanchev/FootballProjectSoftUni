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

namespace FootballProjectSoftUni.Core.Services.Notification
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext data;

        public NotificationService(ApplicationDbContext _data)
        {
            data = _data;
        }
        public async Task<IEnumerable<NotificationViewModel>> AllNotificationsAsync(string userId)
        {
            return await data.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedOn)
                .Select(n => new NotificationViewModel
                {
                    Id = n.Id,
                    Message = n.Message,
                    CreatedOn = n.CreatedOn,
                    IsRead = n.IsRead
                })
                .ToListAsync();
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

            var notifications = coachIds.Select(coachId => new FootballProjectSoftUni.Infrastructure.Data.Models.Notification
            {
                UserId = coachId,
                Message = message,
                CreatedOn = DateTime.Now,
                IsRead = false
            }).ToList();

            await data.Notifications.AddRangeAsync(notifications);
            await data.SaveChangesAsync();
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
    }
}

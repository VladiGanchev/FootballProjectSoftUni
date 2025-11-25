using FootballProjectSoftUni.Core.Models.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Contracts.Notification
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationViewModel>> AllNotificationsAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
        Task CreateNotificationForCityCoachesAsync(int cityId, string message);
        Task MarkAllAsReadAsync(string userId);
    }
}

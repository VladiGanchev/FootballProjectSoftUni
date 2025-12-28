using FootballProjectSoftUni.Core.Models.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;

namespace FootballProjectSoftUni.Core.Contracts.Notification
{
    public interface INotificationService
    {
        Task<IPagedList<NotificationViewModel>> AllNotificationsAsync(string userId, int pageNumber, int pageSize);
        Task<int> GetUnreadCountAsync(string userId);
        Task CreateNotificationForCityCoachesAsync(int cityId, string message);
        Task MarkAllAsReadAsync(string userId);
        Task DeleteAsync(int id, string userId);
    }
}

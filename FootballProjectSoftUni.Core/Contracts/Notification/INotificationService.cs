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
        /// <summary>
        /// A new tournament in the city: a notification for every coach who played there, and an e-mail only for those
        /// who asked for such e-mails (it advertises a paid service).
        /// </summary>
        /// <param name="textFor">The texts for a language ("bg" / "en"); every coach gets them in the language of their latest payment.</param>
        /// <param name="unsubscribeUrl">The page that turns these e-mails off; each e-mail links to it with the coach's own key
        /// (in place of AnnouncementText.UnsubscribeLinkPlaceholder).</param>
        Task CreateNotificationForCityCoachesAsync(int cityId, Func<string, AnnouncementText> textFor, string unsubscribeUrl);
        /// <param name="textFor">The texts for a language ("bg" / "en"); coaches who paid for the tournament in English get English, everybody else Bulgarian.</param>
        Task CreateNotificationForTournamentParticipantsAsync(int tournamentId, Func<string, AnnouncementText> textFor);
        Task MarkAllAsReadAsync(string userId);
        Task DeleteAsync(int id, string userId);
        Task CreateNotificationForUserAsync(string userId, string message);
    }
}

using FootballProjectSoftUni.Core.Contracts.Home;
using FootballProjectSoftUni.Core.Contracts.Notification;
using FootballProjectSoftUni.Core.Services.City;
using FootballProjectSoftUni.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballProjectSoftUni.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService notificationService;

        public NotificationController(INotificationService _notificationService)
        {
            notificationService = _notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> All(int? page)
        {
            //test
            await notificationService.MarkAllAsReadAsync(User.Id());
            var notifications = await notificationService.AllNotificationsAsync(User.Id());
            return View(notifications);
        }
    }
}

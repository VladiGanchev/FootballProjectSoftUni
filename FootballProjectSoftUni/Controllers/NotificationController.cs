using FootballProjectSoftUni.Core.Contracts.Home;
using FootballProjectSoftUni.Core.Contracts.Message;
using FootballProjectSoftUni.Core.Contracts.Notification;
using FootballProjectSoftUni.Core.Models.Notification;
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
        private readonly IContactMessageService messageService;

        public NotificationController(
            INotificationService _notificationService,
            IContactMessageService _messageService)
        {
            notificationService = _notificationService;
            messageService = _messageService;
        }

        [HttpGet]
        public async Task<IActionResult> All(string box = "inbox", int? page = 1)
        {
            var userId = User.Id();
            await notificationService.MarkAllAsReadAsync(userId);

            int pageNumber = page ?? 1;
            int pageSize = 10;

            var model = new NotificationAllPageViewModel
            {
                Box = box
            };

            if (box == "sent")
            {
                model.SentMessages = await messageService.GetSentMessagesAsync(userId, pageNumber, pageSize);
            }
            else
            {
                model.Notifications = await notificationService.AllNotificationsAsync(userId, pageNumber, pageSize);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string box = "inbox", int? page = 1)
        {
            var userId = User.Id();

            await notificationService.DeleteAsync(id, userId);

            return RedirectToAction(nameof(All), new { box, page });
        }
    }
}

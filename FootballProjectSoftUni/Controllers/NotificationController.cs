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
    public async Task<IActionResult> All(string box = "inbox")
    {
        var userId = User.Id();

        await notificationService.MarkAllAsReadAsync(userId);

        var model = new NotificationAllPageViewModel
        {
            Box = box
        };

        if (box == "sent")
        {
            model.SentMessages = await messageService.GetSentMessagesAsync(userId);
        }
        else
        {
            model.Notifications = await notificationService.AllNotificationsAsync(userId);
        }

        return View(model);
    }
}

}

using FootballProjectSoftUni.Core.Contracts.Notification;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FootballProjectSoftUni.Components
{
    public class NotificationComponent : ViewComponent
    {
        private readonly INotificationService notificationService;

        public NotificationComponent(INotificationService _notificationService)
        {
            notificationService = _notificationService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View(0);
            }

            var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            var count = await notificationService.GetUnreadCountAsync(userId);
            return View(count);
        }
    }
}

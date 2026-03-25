using FootballProjectSoftUni.Core.Contracts.Notification;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FootballProjectSoftUni.Components
{
    public class NotificationComponent : ViewComponent
    {
        private readonly INotificationService notificationService;
        private readonly SignInManager<ApplicationUser> signInManager;


        public NotificationComponent(INotificationService _notificationService, SignInManager<ApplicationUser> _signInManager)
        {
            notificationService = _notificationService;
            signInManager = _signInManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!signInManager.IsSignedIn(HttpContext.User))
            {
                return View(0);
            }

            var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            var count = await notificationService.GetUnreadCountAsync(userId);
            return View(count);
        }
    }
}

using FootballProjectSoftUni.Core.Contracts.Coach;
using FootballProjectSoftUni.Core.Contracts.Profile;
using FootballProjectSoftUni.Core.Models.Profile;
using FootballProjectSoftUni.Infrastructure.Data;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FootballProjectSoftUni.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IProfileService profileService;
        private readonly ICoachService coachService;

        public ProfileController(IProfileService profileService, ICoachService coachService)
        {
            this.profileService = profileService;
            this.coachService = coachService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var model = await profileService.GetProfileAsync(userId);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRefereeRole()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await profileService.RemoveRefereeRoleAsync(userId);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveCoachRole()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await coachService.RemoveCoachRoleAsync(userId);

            return RedirectToAction(nameof(Index));
        }
    }
}

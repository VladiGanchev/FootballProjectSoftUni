using FootballProjectSoftUni.Core.Contracts.Coach;
using FootballProjectSoftUni.Core.Contracts.Profile;
using FootballProjectSoftUni.Core.Extensions;
using FootballProjectSoftUni.Core.Models.Profile;
using FootballProjectSoftUni.Core.Services.Team;
using FootballProjectSoftUni.Infrastructure.Data;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using FootballProjectSoftUni.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FootballProjectSoftUni.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IProfileService profileService;
        private readonly ICoachService coachService;
        private readonly IStringLocalizer<SharedResource> localizer;

        public ProfileController(IProfileService profileService, ICoachService coachService, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
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

        // E-mails about new tournaments: only for those who turn them on (they advertise a paid service).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewTournamentEmails(bool on)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await profileService.SetNewTournamentEmailsAsync(userId, on);

            TempData["ProfileMessage"] = localizer[on ? "PrefEmailsSavedOn" : "PrefEmailsSavedOff"].Value;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRefereeRole()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var error = await profileService.RemoveRefereeRoleAsync(userId);

            if (error != null)
            {
                TempData["ErrorMessage"] = localizer.ServiceMessage(error.Message);
                return RedirectToAction("All", "City");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveCoachRole()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await coachService.RemoveCoachRoleAsync(userId);

            if (result.Message != null)
            {
                TempData["ErrorMessage"] = localizer.ServiceMessage(result.Message);
                return RedirectToAction("All", "City");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

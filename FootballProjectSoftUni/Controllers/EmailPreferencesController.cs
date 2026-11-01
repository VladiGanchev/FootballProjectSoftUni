using FootballProjectSoftUni.Core.Contracts.Profile;
using FootballProjectSoftUni.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballProjectSoftUni.Controllers
{
    /// <summary>
    /// The "unsubscribe" link in the e-mails about new tournaments. It works without signing in: the key in the
    /// link proves that it came in the account's own e-mail (UnsubscribeKey).
    /// </summary>
    [AllowAnonymous]
    public class EmailPreferencesController : Controller
    {
        private readonly IProfileService profileService;

        public EmailPreferencesController(IProfileService profileService)
        {
            this.profileService = profileService;
        }

        // Opening the link only asks: mail programs open links by themselves to check them, and that must not
        // unsubscribe anybody.
        [HttpGet]
        public async Task<IActionResult> Unsubscribe(string? u, string? k)
        {
            return View(new UnsubscribeViewModel
            {
                U = u,
                K = k,
                Valid = await profileService.IsUnsubscribeKeyValidAsync(u, k)
            });
        }

        [HttpPost]
        public async Task<IActionResult> Unsubscribe(UnsubscribeViewModel model)
        {
            model.Done = await profileService.UnsubscribeAsync(model.U, model.K);
            model.Valid = model.Done;

            return View(model);
        }
    }
}

#nullable disable

using FootballProjectSoftUni.Core.Contracts.Profile;
using FootballProjectSoftUni.Core.Models.Profile;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using static FootballProjectSoftUni.Infrastructure.Data.Constants.DataConstants;

namespace FootballProjectSoftUni.Areas.Identity.Pages.Account
{
    /// <summary>
    /// Lets a signed-in user delete their own account (password required), by the site's rules.
    /// </summary>
    [Authorize]
    public class DeleteAccountModel : PageModel
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IAccountDeletionService accountDeletionService;
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly ILogger<DeleteAccountModel> logger;

        public DeleteAccountModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IAccountDeletionService accountDeletionService,
            IStringLocalizer<SharedResource> localizer,
            ILogger<DeleteAccountModel> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.accountDeletionService = accountDeletionService;
            this.localizer = localizer;
            this.logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public AccountDeletionCheck Check { get; private set; }

        public class InputModel
        {
            [Required(ErrorMessage = RequireErrorMessage)]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            Check = await accountDeletionService.CheckAsync(user.Id);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            Check = await accountDeletionService.CheckAsync(user.Id);
            if (!Check.CanDelete || !ModelState.IsValid)
            {
                return Page();
            }

            // Wrong passwords count towards the normal lockout (5 attempts), like on the login page.
            var password = await signInManager.CheckPasswordSignInAsync(user, Input.Password, lockoutOnFailure: true);
            if (password.IsLockedOut)
            {
                // As on "change password": whoever doesn't know the password may not be the owner (an unlocked
                // computer), so the session ends too.
                await signInManager.SignOutAsync();
                TempData["ErrorMessage"] = localizer["LoginLockedOut"].Value;   // shown by the login page
                return RedirectToPage("./Login");
            }

            if (!password.Succeeded)
            {
                ModelState.AddModelError(string.Empty, localizer["DAWrongPassword"].Value);
                return Page();
            }

            Check = await accountDeletionService.DeleteAccountAsync(user.Id);
            if (!Check.CanDelete)
            {
                return Page();
            }

            await signInManager.SignOutAsync();
            logger.LogInformation("User {UserId} deleted their account.", user.Id);

            TempData["SuccessMessage"] = localizer["DADeleted"].Value;
            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}

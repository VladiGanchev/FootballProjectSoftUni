#nullable disable

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
    /// Lets a signed-in user change their own password (current password required).
    /// </summary>
    [Authorize]
    public class ChangePasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IStringLocalizer<SharedResource> localizer;

        public ChangePasswordModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IStringLocalizer<SharedResource> localizer)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.localizer = localizer;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = RequireErrorMessage)]
            [DataType(DataType.Password)]
            public string OldPassword { get; set; }

            [Required(ErrorMessage = RequireErrorMessage)]
            [StringLength(100, ErrorMessage = StringLengthErrorMessage, MinimumLength = 8)]
            [DataType(DataType.Password)]
            public string NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Compare(nameof(NewPassword), ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            // A wrong current password counts like a wrong password at sign-in, so somebody sitting in another
            // person's open session can't guess it here without limit: after 5 wrong ones the account is locked
            // for 15 minutes and this session ends. (Administrators are never locked out - see Login.)
            var check = await signInManager.CheckPasswordSignInAsync(user, Input.OldPassword, lockoutOnFailure: true);
            if (check.IsLockedOut)
            {
                await signInManager.SignOutAsync();
                TempData["ErrorMessage"] = localizer["LoginLockedOut"].Value;   // shown by the login page
                return RedirectToPage("./Login");
            }

            if (!check.Succeeded)
            {
                ModelState.AddModelError(string.Empty, localizer["CPWrongCurrent"].Value);
                return Page();
            }

            var result = await userManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    var message = error.Code == "PasswordMismatch"
                        ? localizer["CPWrongCurrent"].Value
                        : error.Description;

                    ModelState.AddModelError(string.Empty, message);
                }

                return Page();
            }

            // Keep the user signed in with the new security stamp.
            await signInManager.RefreshSignInAsync(user);

            StatusMessage = localizer["CPChanged"].Value;
            return RedirectToPage();
        }
    }
}

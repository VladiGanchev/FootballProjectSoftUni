using System.ComponentModel.DataAnnotations;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using FootballProjectSoftUni.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static FootballProjectSoftUni.Infrastructure.Data.Constants.DataConstants;

namespace FootballProjectSoftUni.Areas.Identity.Pages.Account
{
    /// <summary>A new "confirm your e-mail" link, when the first one got lost or expired.</summary>
    [AllowAnonymous]
    public class ResendEmailConfirmationModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ConfirmationEmailSender _confirmationEmail;

        public ResendEmailConfirmationModel(UserManager<ApplicationUser> userManager, ConfirmationEmailSender confirmationEmail)
        {
            _userManager = userManager;
            _confirmationEmail = confirmationEmail;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public bool Sent { get; private set; }

        public class InputModel
        {
            [Required(ErrorMessage = RequireErrorMessage)]
            [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
            public string Email { get; set; } = string.Empty;
        }

        public void OnGet(string? email = null)
        {
            Input.Email = email ?? string.Empty;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // The same answer whether the address has an account or not: nobody learns which e-mails are registered.
            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user != null && !await _userManager.IsEmailConfirmedAsync(user))
            {
                await _confirmationEmail.SendAsync(user, Url, Request);
            }

            Sent = true;
            return Page();
        }
    }
}

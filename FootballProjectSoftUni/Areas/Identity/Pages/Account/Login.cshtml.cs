// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Localization;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using static FootballProjectSoftUni.Core.Constants.RoleConstants;
using static FootballProjectSoftUni.Infrastructure.Data.Constants.DataConstants;

namespace FootballProjectSoftUni.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public LoginModel(SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger, UserManager<ApplicationUser> userManager, IStringLocalizer<SharedResource> localizer)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
            _localizer = localizer;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>The password was right but the e-mail isn't confirmed: the page offers a new link.</summary>
        public bool NotConfirmed { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage = RequireErrorMessage)]
            [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage = RequireErrorMessage)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "RememberMe")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl = LocalOrHome(returnUrl);

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        // Only addresses of this site: a link with a foreign returnUrl used to end with an error page after logging in.
        private string LocalOrHome(string returnUrl) =>
            Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Content("~/");

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = LocalOrHome(returnUrl);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(Input.Email);

                // Administrators are never locked out: anybody who knows the address could otherwise keep the
                // organiser out of the site with a few wrong passwords. Their guessing is still limited per IP
                // address (AuthRateLimitMiddleware) and the admin password has to be long.
                var isAdmin = existingUser != null && await _userManager.IsInRoleAsync(existingUser, AdminRole);

                // Accounts created before lockout was turned on have it disabled: switch it on for everybody else,
                // and off for administrators (which also lifts a lockout they already got).
                if (existingUser != null && existingUser.LockoutEnabled == isAdmin)
                {
                    await _userManager.SetLockoutEnabledAsync(existingUser, !isAdmin);
                }

                // Wrong passwords count towards lockout (5 attempts -> 15 minutes, see ServiceCollectionExtension).
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: !isAdmin);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    // The site has no two-factor pages (they are switched off), so nobody can turn 2FA on.
                    ModelState.AddModelError(string.Empty, _localizer["LoginTwoFactorUnsupported"].Value);
                    return Page();
                }
                if (result.IsNotAllowed && existingUser != null)
                {
                    // The e-mail isn't confirmed yet. Identity stops before it looks at the password or the lockout,
                    // so both are checked here - otherwise the password of such an account could be guessed without limit.
                    if (await _userManager.IsLockedOutAsync(existingUser))
                    {
                        ModelState.AddModelError(string.Empty, _localizer["LoginLockedOut"].Value);
                        return Page();
                    }

                    if (await _userManager.CheckPasswordAsync(existingUser, Input.Password))
                    {
                        // The right password. (With a wrong one the answer stays "wrong e-mail or password",
                        // so strangers don't learn which addresses have an account.)
                        NotConfirmed = true;
                        ModelState.AddModelError(string.Empty, _localizer["LoginNotConfirmed"].Value);
                        return Page();
                    }

                    if (!isAdmin)
                    {
                        await _userManager.AccessFailedAsync(existingUser);

                        if (await _userManager.IsLockedOutAsync(existingUser))
                        {
                            ModelState.AddModelError(string.Empty, _localizer["LoginLockedOut"].Value);
                            return Page();
                        }
                    }
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    ModelState.AddModelError(string.Empty, _localizer["LoginLockedOut"].Value);
                    return Page();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, _localizer["LoginInvalid"].Value);
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}

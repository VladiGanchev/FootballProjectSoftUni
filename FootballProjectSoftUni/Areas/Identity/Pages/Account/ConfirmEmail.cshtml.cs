// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Text;
using System.Threading.Tasks;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace FootballProjectSoftUni.Areas.Identity.Pages.Account
{
    /// <summary>
    /// The link from the registration e-mail. Replaces the built-in page, which was only in English
    /// and showed an error page for a damaged link.
    /// </summary>
    [AllowAnonymous]
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ConfirmEmailModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public bool Confirmed { get; private set; }

        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return LocalRedirect("~/");
            }

            // A link of a deleted account or a damaged link (e.g. cut by the mail program) is just an invalid link.
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Page();
            }

            try
            {
                code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            }
            catch (FormatException)
            {
                return Page();
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);
            Confirmed = result.Succeeded;

            return Page();
        }
    }
}

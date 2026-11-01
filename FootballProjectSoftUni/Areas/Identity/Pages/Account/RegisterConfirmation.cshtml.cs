using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FootballProjectSoftUni.Areas.Identity.Pages.Account
{
    /// <summary>"Check your e-mail" after registering: the account can be used once the link in it is opened.</summary>
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        // Comes from the registration (TempData), never from the URL.
        public string? Email { get; private set; }

        public void OnGet()
        {
            Email = TempData["RegisteredEmail"] as string;
        }
    }
}

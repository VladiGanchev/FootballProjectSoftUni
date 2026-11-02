using FootballProjectSoftUni.Core.Contracts.City;
using FootballProjectSoftUni.Core.Contracts.Home;
using FootballProjectSoftUni.Core.Contracts.Team;
using FootballProjectSoftUni.Core.Contracts.Tournament;
using FootballProjectSoftUni.Extensions;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using FootballProjectSoftUni.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHomeService homeService;
        private readonly SignInManager<ApplicationUser> signInManager;

        public HomeController(ILogger<HomeController> logger, IHomeService _homeService, SignInManager<ApplicationUser> _signInManager)
        {
            _logger = logger;
            homeService = _homeService;
            signInManager = _signInManager;

        }

        public async Task<IActionResult> Index()
        {
            if(signInManager.IsSignedIn(User))
            {
                return RedirectToAction("All", "City");
            }

            var homePageModel = await homeService.GetHomePageData();

            return View(homePageModel);
        }

        [HttpGet]
        public IActionResult Faq()
        {
            return View();
        }

        // Terms.cshtml / Privacy.cshtml are in Bulgarian; Terms.en.cshtml / Privacy.en.cshtml are picked for English.
        [HttpGet]
        public IActionResult Terms()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Privacy()
        {
            return View();
        }

        // The error page is shown again for the failed request - a POST when a form was rejected - so it must not
        // check the form token itself (that check failing again used to leave a blank page).
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statusCode)
        {
            if (statusCode == 400 && HttpContext.Items.ContainsKey(AntiforgeryFailureMarker.ItemKey))
            {
                // The sign-in form sent twice (a double click): the first one signed the person in, so the token of
                // the second one, made for a guest, no longer matches. Nothing to explain - carry on, signed in.
                var original = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
                if (User.Identity?.IsAuthenticated == true && original != null
                    && string.Equals(original.OriginalPath, "/Identity/Account/Login", StringComparison.OrdinalIgnoreCase))
                {
                    var returnUrl = QueryHelpers.ParseQuery(original.OriginalQueryString)
                        .FirstOrDefault(q => string.Equals(q.Key, "returnUrl", StringComparison.OrdinalIgnoreCase)).Value.ToString();

                    return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "/");
                }

                // Back to the form (a fresh copy with a new token), when it came from this site.
                var referer = Request.Headers["Referer"].ToString();
                var refererUri = Uri.TryCreate(referer, UriKind.Absolute, out var uri) && uri.Host == Request.Host.Host
                    ? uri.PathAndQuery
                    : null;

                ViewBag.RetryUrl = refererUri != null && Url.IsLocalUrl(refererUri) ? refererUri : Url.Action(nameof(Index));
                return View("ErrorFormExpired");
            }

            if (statusCode == 401)
            {
                return View("Error401");
            }

            if (statusCode == 404)
            {
                return View("Error404");
            }

            if (statusCode == 429)
            {
                return View("Error429");
            }

            return View();
        }

        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                // Only the server reads it; Secure and SameSite come from the cookie policy (Program.cs).
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), HttpOnly = true, IsEssential = true }
            );

            // Only addresses of this site (a foreign returnUrl used to end with an error page).
            return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl : "/");
        }
    }
}
using FootballProjectSoftUni.Core.Contracts.City;
using FootballProjectSoftUni.Core.Contracts.Home;
using FootballProjectSoftUni.Core.Contracts.Team;
using FootballProjectSoftUni.Core.Contracts.Tournament;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using FootballProjectSoftUni.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statusCode)
        {

            if (statusCode == 401)
            {
                return View("Error401");
            }

            if (statusCode == 404)
            {
                return View("Error404");
            }

            return View();
        }

        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
        }
    }
}
using FootballProjectSoftUni.Core.Contracts.City;
using FootballProjectSoftUni.Core.Contracts.Home;
using FootballProjectSoftUni.Core.Contracts.Team;
using FootballProjectSoftUni.Core.Contracts.Tournament;
using FootballProjectSoftUni.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHomeService homeService;

        public HomeController(ILogger<HomeController> logger, IHomeService _homeService)
        {
            _logger = logger;
            homeService = _homeService;
        }

        public async Task<IActionResult> Index()
        {
            if(User.Identity.IsAuthenticated)
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
    }
}
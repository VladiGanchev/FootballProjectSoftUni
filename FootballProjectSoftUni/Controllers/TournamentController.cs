using Microsoft.AspNetCore.Mvc;

namespace FootballProjectSoftUni.Controllers
{
    public class TournamentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

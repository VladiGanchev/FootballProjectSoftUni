using Microsoft.AspNetCore.Mvc;

namespace FootballProjectSoftUni.Controllers
{
    public class CoachController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

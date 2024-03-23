using Microsoft.AspNetCore.Mvc;

namespace FootballProjectSoftUni.Controllers
{
    public class RefereeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

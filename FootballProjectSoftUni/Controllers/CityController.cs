using FootballProjectSoftUni.Core.Contracts.City;
using Microsoft.AspNetCore.Mvc;

namespace FootballProjectSoftUni.Controllers
{
    public class CityController : Controller
    {
        private readonly ICityService cityService;

        public CityController(ICityService _service)
        {
            cityService = _service;
        }

        public async Task<IActionResult> All(int? page)
        {

            var cities = await cityService.All(page);
            return View(cities);
        }
    }
}

using FootballProjectSoftUni.Core.Contracts.City;
using FootballProjectSoftUni.Core.Models.City;
using FootballProjectSoftUni.Infrastructure.Data.Models;
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

        [HttpGet]
        public async Task<IActionResult> All(int? page)
        {

            var cities = await cityService.AllCitiesAsync(page);
            return View(cities);
        }

        [HttpGet]
        public IActionResult AddCity()
        {
            var model = new CityViewModel();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddCity(CityViewModel model)
        {
            if (ModelState.IsValid == false)
            {
                return View(model);
            }

            await cityService.AddCityAsync(model);

            return RedirectToAction(nameof(All));
        }

        [HttpGet]

        public async Task<IActionResult> DeleteCity()
        {

            var cities = await cityService.AllCitiesAsync(); 

            return View(cities);
        }


        [HttpGet]

        public async Task<IActionResult> DeleteConfirmed(CityViewModel model, int id)
        {
            var town = await cityService.FindTownAsync(model, id);

            if (town == null)
            {
                return BadRequest();
            }

            return View(town);

        }

        [HttpPost]

        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await cityService.DeleteCityAsync(id);

            return RedirectToAction(nameof(All));
        }

        [HttpGet]
        public async Task<IActionResult> Search(string searchString)
        {
            var cities = await cityService.SearchAsync(searchString);

            return View(cities);
        }
    }
}

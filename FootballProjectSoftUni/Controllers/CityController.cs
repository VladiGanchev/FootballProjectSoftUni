using FootballProjectSoftUni.Core.Contracts.City;
using FootballProjectSoftUni.Core.Models.City;
using FootballProjectSoftUni.Extensions;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballProjectSoftUni.Controllers
{
    [Authorize]
    public class CityController : Controller
    {
        private readonly ICityService cityService;

        public CityController(ICityService _service)
        {
            cityService = _service;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> All(int? page)
        {
            var cities = await cityService.AllCitiesAsync(page);
            return View(cities);
        }

        [HttpGet]
        public IActionResult AddCity()
        {
            if (User.IsAdmin() == false)
            {
                return Unauthorized();
            }

            var model = new CityViewModel();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddCity(CityViewModel model)
        {
            if (User.IsAdmin() == false)
            {
                return Unauthorized();
            }

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
            if (User.IsAdmin() == false)
            {
                return Unauthorized();
            }

            var cities = await cityService.AllCitiesAsync(); 

            return View(cities);
        }


        [HttpGet]

        public async Task<IActionResult> DeleteConfirmed(CityViewModel model, int id)
        {
            if (User.IsAdmin() == false)
            {
                return Unauthorized();
            }

            var town = await cityService.FindTownAsync(model, id);

            if (town == null)
            {
                return NotFound();
            }

            return View(town);

        }

        [HttpPost]

        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (User.IsAdmin() == false)
            {
                return Unauthorized();
            }

            await cityService.DeleteCityAsync(id);

            return RedirectToAction(nameof(All));
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Search(string searchString)
        {
            var cities = await cityService.SearchAsync(searchString);

            return View(cities);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetBestTeams(int id)
        {
            var bestTeams = await cityService.GetBestTeamsAsync(id);
            return View(bestTeams);
        }

        [Authorize] // по желание можеш да сложиш и проверка за Admin с IsAdmin()
        [HttpGet]
        public async Task<IActionResult> UpdateCityLeaders()
        {
            if (!User.IsAdmin())
            {
                return Unauthorized();
            }

            var model = await cityService.GetUpdateCityBestTeamFormAsync();
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateCityLeaders(UpdateCityBestTeamViewModel model)
        {
            if (!User.IsAdmin())
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                // трябва да върнем пак списъците за dropdown-ите
                var refill = await cityService.GetUpdateCityBestTeamFormAsync();
                model.Cities = refill.Cities;
                model.Teams = refill.Teams;

                return View(model);
            }

            await cityService.IncrementTeamWinsInCityAsync(model.CityId, model.TeamId);

            // по желание – пренасочваме към класацията на този град
            return RedirectToAction("GetBestTeams", new { id = model.CityId });
        }

    }
}

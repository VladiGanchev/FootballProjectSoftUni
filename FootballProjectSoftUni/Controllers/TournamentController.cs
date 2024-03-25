using FootballProjectSoftUni.Core.Contracts.Tournament;
using FootballProjectSoftUni.Core.Models.City;
using FootballProjectSoftUni.Core.Models.Tournament;
using FootballProjectSoftUni.Core.Services.Tournament;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using static FootballProjectSoftUni.Infrastructure.Data.Constants.DataConstants;

namespace FootballProjectSoftUni.Controllers
{
    public class TournamentController : Controller
    {
        private readonly ITournamentService service;

        public TournamentController(ITournamentService _service)
        {
            service = _service;
        }

        [HttpGet]
        public async Task<IActionResult> CityTournaments(int id)
        {
            var city = await service.FindCityAsync(id);

            if (city == null)
            {
                return BadRequest();
            }

            var tournaments = await service.GetCityTournamentsAsync(id);

            return View(tournaments);
        }

        [HttpGet]
        public async Task<IActionResult> AddTournamentToCity()
        {
            var tournament = new AddTournamentFormViewModel();

            var cities = await service.GetCitiesAsync();
            tournament.Cities = cities;

            return View(tournament);
        }

        [HttpPost]

        public async Task<IActionResult> AddTournamentToCity(AddTournamentFormViewModel model, int cityId)
        {
            DateTime start = DateTime.Now;
            DateTime end = DateTime.Now;

            if (!DateTime.TryParseExact(model.StartDate, RequiredDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out start))
            {
                ModelState.AddModelError(nameof(model.StartDate), $"Invalid date, format must be {RequiredDateTimeFormat}.");
            }

            if (!DateTime.TryParseExact(model.EndDate, RequiredDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out end))
            {
                ModelState.AddModelError(nameof(model.EndDate), $"Invalid date, format must be {RequiredDateTimeFormat}.");
            }

            if (!ModelState.IsValid)
            {
                var cities = await service.GetCitiesAsync();
                model.Cities = cities;
                return View(model);
            }

            await service.AddTournamentToCityAsync(model, cityId, start, end);

            return RedirectToAction("All", "City");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var tournament = await service.GetTournamentDetailsAsync(id);

            if (tournament == null)
            {
                return BadRequest();
            }

            return View(tournament);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var tournament = await service.FindTournamentAsync(id);

            if (tournament == null)
            {
                return BadRequest();
            }

            return View(tournament);


        }

        [HttpPost]

        public async Task<IActionResult> Edit(EditViewModel model)
        {
            DateTime start = DateTime.Now;
            DateTime end = DateTime.Now;
            

            if (!DateTime.TryParseExact(model.StartDate, RequiredDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out start))
            {
                ModelState.AddModelError(nameof(model.StartDate), $"Invalid date, format must be {RequiredDateTimeFormat}.");
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
            }

            if (!DateTime.TryParseExact(model.EndDate, RequiredDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out end))
            {
                ModelState.AddModelError(nameof(model.EndDate), $"Invalid date, format must be {RequiredDateTimeFormat}.");
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
            }

            var tournament = await service.FindTournamentByIdAsync(model.Id);

            if (tournament == null)
            {
                return BadRequest();
            }

            await service.EditTournamentAsync(model, start, end);

            var cityId = tournament.TournamentCities.FirstOrDefault().CityId;



            return RedirectToAction("CityTournaments", "Tournament", new { id = cityId });

        }

        public async Task<IActionResult> Delete(int id)
        {
           var result = await service.DeleteTournamentAsync(id);

            if (result == false)
            {
                return BadRequest();
            }
            return RedirectToAction("All", "City");
        }
    }
}

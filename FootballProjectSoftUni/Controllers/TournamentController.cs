using FootballProjectSoftUni.Core.Contracts.Notification;
using FootballProjectSoftUni.Core.Contracts.Tournament;
using FootballProjectSoftUni.Core.Extensions;
using FootballProjectSoftUni.Core.Models.City;
using FootballProjectSoftUni.Core.Models.Notification;
using FootballProjectSoftUni.Core.Models.Tournament;
using FootballProjectSoftUni.Core.Services.Tournament;
using FootballProjectSoftUni.Extensions;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using static FootballProjectSoftUni.Infrastructure.Data.Constants.DataConstants;

namespace FootballProjectSoftUni.Controllers
{
    [Authorize]
    public class TournamentController : Controller
    {
        private readonly ITournamentService service;
        private readonly INotificationService notificationService;

        public TournamentController(
            ITournamentService _service,
            INotificationService _notificationService)
        {
            service = _service;
            notificationService = _notificationService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> CityTournaments(int id, bool showPast)
        {
            var city = await service.FindCityAsync(id);

            if (city == null)
            {
                return BadRequest();
            }

            var tournaments = await service.GetCityTournamentsAsync(id, showPast);

            ViewBag.ShowPast = showPast;
            ViewBag.CityId = id;

            return View(tournaments);
        }

        [HttpGet]
        public async Task<IActionResult> AddTournamentToCity()
        {
            if (User.IsAdmin() == false)
            {
                return Unauthorized();
            }

            var tournament = new AddTournamentFormViewModel();

            var cities = await service.GetCitiesAsync();
            tournament.Cities = cities;

            return View(tournament);
        }

        [HttpPost]

        public async Task<IActionResult> AddTournamentToCity(AddTournamentFormViewModel model, int cityId)
        {
            if (User.IsAdmin() == false)
            {
                return Unauthorized();
            }

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

            var city = await service.FindCityAsync(cityId);
            if (city != null)
            {
                string message = $"Създаден е нов турнир в {city.Name}! Регистрирай се сега.";
                await notificationService.CreateNotificationForCityCoachesAsync(cityId, message);
            }

            return RedirectToAction("All", "City");
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Details(int id, string information)
        {
            var tournament = await service.GetTournamentDetailsAsync(id);

            if (tournament == null)
            {
                return BadRequest();
            }

            if (information != tournament.GetInformation())
            {
                return BadRequest();
            }

            return View(tournament);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (User.IsAdmin() == false)
            {
                return Unauthorized();
            }

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
            if (User.IsAdmin() == false)
            {
                return Unauthorized();
            }

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
            if (User.IsAdmin() == false)
            {
                return Unauthorized();
            }

            var result = await service.DeleteTournamentAsync(id);

            if (result == false)
            {
                return BadRequest();
            }
            return RedirectToAction("All", "City");
        }
    }
}

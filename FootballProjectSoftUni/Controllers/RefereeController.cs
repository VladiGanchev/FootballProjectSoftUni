using FootballProjectSoftUni.Core.Contracts.Referee;
using FootballProjectSoftUni.Core.Contracts.Tournament;
using FootballProjectSoftUni.Core.Models.Referee;
using FootballProjectSoftUni.Extensions;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;
using static FootballProjectSoftUni.Infrastructure.Data.Constants.DataConstants;

namespace FootballProjectSoftUni.Controllers
{
    [Authorize]
    public class RefereeController : Controller
    {
        private readonly IRefereeService refereeService;
        private readonly ITournamentService tournamentService;

        public RefereeController(IRefereeService _refereeService, ITournamentService _tournamentService)
        {
            refereeService = _refereeService;
            tournamentService = _tournamentService;
        }

        [HttpGet]
        public async Task<IActionResult> BecomeReferee(int id)
        {
            string userId = User.Id();

            var result = await refereeService.CheckForErrorsAsync(id, userId);

            if (result != null)
            {
                ModelState.AddModelError("", result.Message);
                TempData["ErrorMessage"] = result.Message;

                var tournament = await tournamentService.FindTournamentByIdAsync(id);

                var cityId = tournament.TournamentCities.FirstOrDefault().CityId;


                return RedirectToAction("CityTournaments", "Tournament", new { id = cityId });
            }

            var model = new RefereeFormViewMOdel();

            return View(model);
        }

        [HttpPost]

        public async Task<IActionResult> BecomeReferee(RefereeFormViewMOdel model, int id)
        {
            DateTime birthdate = DateTime.Now;

            if (!DateTime.TryParseExact(model.Birthdate, RequiredDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out birthdate))
            {
                ModelState.AddModelError(nameof(model.Birthdate), $"Invalid date, format must be {RequiredDateTimeFormat}.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int age = CalculateAge(birthdate);

            if (age < 18)
            {
                ModelState.AddModelError(nameof(model.Birthdate), "You must be at least 18 years old to become a referee.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.Id();

            var result = await refereeService.CreateRefereeToTournamentAsync(model, id, userId, birthdate);

            if (result == false)
            {
                return BadRequest();
            }

            return RedirectToAction(nameof(AllTournamentsToReferee));

        }

        [HttpGet]
        public async Task<IActionResult> AllTournamentsToReferee()
        {
            var userId = User.Id();

            var model = await refereeService.GetTournamentsAsync(userId);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> LeaveTournament(int id)
        {
            var userId = User.Id();

            var result = await refereeService.LeaveTournamentAsync(id, userId);

            if (result == false)
            {
                return BadRequest();
            }

            var tournament = await tournamentService.FindTournamentByIdAsync(id);

            var cityId = tournament.TournamentCities.FirstOrDefault().CityId;


            return RedirectToAction("CityTournaments", "Tournament", new { id = cityId });

        }
        public static int CalculateAge(DateTime birthdate)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - birthdate.Year;
            if (birthdate.Date > today.AddYears(-age))
            {
                age--; 
            }
            return age;
        }
    }
}

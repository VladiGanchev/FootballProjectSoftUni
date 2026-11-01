using FootballProjectSoftUni.Core.Contracts.Coach;
using FootballProjectSoftUni.Core.Contracts.Tournament;
using FootballProjectSoftUni.Core.Extensions;
using FootballProjectSoftUni.Core.Models.Coach;
using FootballProjectSoftUni.Core.Services.Referee;
using FootballProjectSoftUni.Core.Services.Tournament;
using FootballProjectSoftUni.Extensions;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;

namespace FootballProjectSoftUni.Controllers
{
    [Authorize]
    public class CoachController : Controller
    {
        private readonly ICoachService coachService;
        private readonly ITournamentService tournamentService;
        private readonly IStringLocalizer<SharedResource> localizer;

        public CoachController(ICoachService _coachService, ITournamentService _tournamentService, IStringLocalizer<SharedResource> _localizer)
        {
            localizer = _localizer;
            coachService = _coachService;
            tournamentService = _tournamentService;
        }

        [HttpGet]
        public async Task<IActionResult> BecomeCoach(int? tournamentId)
        {
            var userId = User.Id();

            var result = await coachService.CheckForErrorsAsync(userId);

            if (result != null)
            {
                TempData["ErrorMessage"] = localizer.ServiceMessage(result.Message);

                return RedirectToAction("All", "City");
            }

            var model = new CoachViewModel();
            ViewBag.TournamentId = tournamentId;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> BecomeCoach(CoachViewModel model, int? tournamentId)
        {
            var checkError = await coachService.CheckForErrorsAsync(User.Id());
            if (checkError != null)
            {
                TempData["ErrorMessage"] = localizer.ServiceMessage(checkError.Message);
                return RedirectToAction("All", "City");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.TournamentId = tournamentId;
                return View(model);
            }

            string userId = User.Id();

            await coachService.BecomeCoachAsync(model, userId);

            if (tournamentId.HasValue)
            {
                return RedirectToAction("JoinTeam", "Team", new { id = tournamentId.Value });
            }

            return RedirectToAction("All", "City");
        }

        [HttpGet]
        public async Task<IActionResult> AllTournamentsToParticipateAsCoach()
        {
            string userId = User.Id();


            var tournaments = await coachService.GetAllTournamentsToParticipateAsCoachAsync(userId);
            return View(tournaments);
        }

        [HttpPost]
        public async Task<IActionResult> LeaveTournament(int id)
        {
            var userId = User.Id();

            var result = await coachService.LeaveTournamentAsync(id, userId);

            if (result == false)
            {
                // Started already (leaving is possible only before the start) or not taking part.
                TempData["ErrorMessage"] = localizer["LeaveNotAllowed"].Value;
                return RedirectToAction(nameof(AllTournamentsToParticipateAsCoach));
            }

            var tournament = await tournamentService.FindTournamentByIdAsync(id);

            var cityId = tournament.TournamentCities.FirstOrDefault()?.CityId;

            // The fee is not returned automatically: the coach has to ask the organiser.
            TempData["SuccessMessage"] = localizer["LeftTournamentNoRefund"].Value;
            return RedirectToAction("CityTournaments", "Tournament", new { id = cityId });

        }
    }
}

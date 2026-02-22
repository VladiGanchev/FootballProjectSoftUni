using FootballProjectSoftUni.Core.Contracts.Coach;
using FootballProjectSoftUni.Core.Contracts.Tournament;
using FootballProjectSoftUni.Core.Models.Coach;
using FootballProjectSoftUni.Core.Services.Referee;
using FootballProjectSoftUni.Core.Services.Tournament;
using FootballProjectSoftUni.Extensions;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FootballProjectSoftUni.Controllers
{
    [Authorize]
    public class CoachController : Controller
    {
        private readonly ICoachService coachService;
        private readonly ITournamentService tournamentService;
        

        public CoachController(ICoachService _coachService, ITournamentService _tournamentService)
        {
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
                ModelState.AddModelError("", result.Message);
                TempData["ErrorMessage"] = result.Message;

                return RedirectToAction("All", "City");
            }

            var model = new CoachViewModel();
            ViewBag.TournamentId = tournamentId;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> BecomeCoach(CoachViewModel model, int? tournamentId)
        {
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

        [HttpGet]
        public async Task<IActionResult> LeaveTournament(int id)
        {
            var userId = User.Id();

            var result = await coachService.LeaveTournamentAsync(id, userId);

            if (result == false)
            {
                return BadRequest();
            }

            var tournament = await tournamentService.FindTournamentByIdAsync(id);

            var cityId = tournament.TournamentCities.FirstOrDefault()?.CityId;

            return RedirectToAction("CityTournaments", "Tournament", new { id = cityId });

        }
    }
}

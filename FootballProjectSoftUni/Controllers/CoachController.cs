using FootballProjectSoftUni.Core.Contracts.Coach;
using FootballProjectSoftUni.Core.Contracts.Tournament;
using FootballProjectSoftUni.Core.Models.Coach;
using FootballProjectSoftUni.Core.Services.Tournament;
using FootballProjectSoftUni.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FootballProjectSoftUni.Controllers
{
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
        public IActionResult BecomeCoach()
        {
            var model = new CoachViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> BecomeCoach(CoachViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string userId = User.Id();

            await coachService.BecomeCoachAsync(model, userId);
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

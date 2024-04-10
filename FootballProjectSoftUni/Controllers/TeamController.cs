using FootballProjectSoftUni.Core.Contracts.Team;
using FootballProjectSoftUni.Core.Contracts.Tournament;
using FootballProjectSoftUni.Core.Models.Team;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;
using FootballProjectSoftUni.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace FootballProjectSoftUni.Controllers
{
    [Authorize]
    public class TeamController : Controller
    {
        private readonly ITeamService teamService;
        private readonly ITournamentService tournamentService;

        public TeamController(ITeamService _teamService, ITournamentService _tournamentService)
        {
            teamService = _teamService;
            tournamentService = _tournamentService;
        }

        [HttpGet]
        public async Task<IActionResult> JoinTeam(int id)
        {
            string userId = User.Id();

            var error = await teamService.CheckForErrorsAsync(id, userId);

            if (error != null)
            {
                if (error.Message == "You need to become a coach to join a team.")
                {
                    return RedirectToAction("BecomeCoach", "Coach");
                }
                else
                {
                    var cityId = await teamService.GetCityIdAsync(id);

                    ModelState.AddModelError("", error.Message);
                    TempData["ErrorMessage"] = error.Message;
                    return RedirectToAction("CityTournaments", "Tournament", new { id = cityId });
                }

            }

            var model = teamService.CreateModel(id);

            return View(model);

        }

        [HttpPost]
        public async Task<IActionResult> JoinTeam(TeamRegistrationViewModel viewModel, int id)
        {
            var userId = User.Id();

            var result = await teamService.JoinTeamAsync(viewModel, id, userId);

            if (result != null)
            {
                if (result.Message == "BadRequest Message")
                {
                    return BadRequest();
                }
                else
                {
                    ModelState.AddModelError("", result.Message);
                    if (!ModelState.IsValid)
                    {
                        return View(viewModel);
                    }
                }

            }

            var tournament = await tournamentService.FindTournamentByIdAsync(id);

            var cityId = tournament.TournamentCities.FirstOrDefault().CityId;

            return RedirectToAction("CityTournaments", "Tournament", new { id = cityId });

        }
    }
}

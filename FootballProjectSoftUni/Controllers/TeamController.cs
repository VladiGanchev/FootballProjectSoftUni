using FootballProjectSoftUni.Core.Contracts.Payment;
using FootballProjectSoftUni.Core.Contracts.Team;
using FootballProjectSoftUni.Core.Contracts.Tournament;
using FootballProjectSoftUni.Core.Models.Team;
using FootballProjectSoftUni.Core.Services.Payment;
using FootballProjectSoftUni.Extensions;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;

namespace FootballProjectSoftUni.Controllers
{
    [Authorize]
    public class TeamController : Controller
    {
        private readonly ITeamService teamService;
        private readonly ITournamentService tournamentService;
        private readonly IPaymentService paymentService;

        public TeamController(ITeamService _teamService, ITournamentService _tournamentService, IPaymentService _paymentService)
        {
            teamService = _teamService;
            tournamentService = _tournamentService;
            paymentService = _paymentService;
        }

        [HttpGet]
        public async Task<IActionResult> JoinTeam(int id)
        {
            string userId = User.Id();

            // 1) Първо глобалните проверки (макс. отбори, вече е коуч в турнир, рефер и т.н.)
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

            // If coach already has a team -> start Stripe payment immediately
            // You can add a method teamService.GetCoachTeamId(userId) or read in service.
            var coachTeamId = await teamService.GetCoachTeamIdAsync(userId); // implement simple helper
            if (coachTeamId.HasValue)
            {
                var url = await paymentService.CreateTournamentJoinCheckoutAsync(id, userId, coachTeamId.Value);
                return Redirect(url);
            }

            // else show team creation form
            var model = teamService.CreateModel(id);
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> JoinTeam(TeamRegistrationViewModel viewModel, int id)
        {
            var userId = User.Id();

            var error = await teamService.CheckForErrorsAsync(id, userId);
            if (error != null)
            {
                if (error.Message == "You need to become a coach to join a team.")
                    return RedirectToAction("BecomeCoach", "Coach");

                var cityId = await teamService.GetCityIdAsync(id);
                TempData["ErrorMessage"] = error.Message;
                return RedirectToAction("CityTournaments", "Tournament", new { id = cityId });
            }

            if (!ModelState.IsValid)
                return View(viewModel);

            int teamId;
            try
            {
                teamId = await teamService.CreateTeamDraftAsync(viewModel, userId);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(viewModel);
            }

            var url = await paymentService.CreateTournamentJoinCheckoutAsync(id, userId, teamId);
            return Redirect(url);
        }

    }
}

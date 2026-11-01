using FootballProjectSoftUni.Core.Constants;
using FootballProjectSoftUni.Core.Contracts.Payment;
using FootballProjectSoftUni.Core.Contracts.Team;
using FootballProjectSoftUni.Core.Contracts.Tournament;
using FootballProjectSoftUni.Core.Extensions;
using FootballProjectSoftUni.Core.Models.Payment;
using FootballProjectSoftUni.Core.Models.Player;
using FootballProjectSoftUni.Core.Models.Team;
using FootballProjectSoftUni.Core.Services.Payment;
using FootballProjectSoftUni.Extensions;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;
using Microsoft.Extensions.Localization;
using static FootballProjectSoftUni.Infrastructure.Data.Constants.DataConstants;

namespace FootballProjectSoftUni.Controllers
{
    [Authorize]
    public class TeamController : Controller
    {
        private readonly ITeamService teamService;
        private readonly ITournamentService tournamentService;
        private readonly IPaymentService paymentService;
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly ILogger<TeamController> logger;

        public TeamController(ITeamService _teamService, ITournamentService _tournamentService, IPaymentService _paymentService, IStringLocalizer<SharedResource> localizer, ILogger<TeamController> logger)
        {
            this.logger = logger;
            teamService = _teamService;
            tournamentService = _tournamentService;
            paymentService = _paymentService;
            this.localizer = localizer;
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
                    return RedirectToAction("BecomeCoach", "Coach", new { tournamentId = id });
                }
                else
                {
                    var cityId = await teamService.GetCityIdAsync(id);

                    TempData["ErrorMessage"] = localizer.ServiceMessage(error.Message);
                    return RedirectToAction("CityTournaments", "Tournament", new { id = cityId });
                }
            }

            var coachTeamId = await teamService.GetCoachTeamIdAsync(userId); 
            if (coachTeamId.HasValue)
            {
                return RedirectToAction("Declaration", new
                {
                    tournamentId = id,
                    teamId = coachTeamId.Value
                });
            }

            var model = teamService.CreateModel(id);
            return View(model);
        }


        // The form always shows a field for every player; a request with fewer of them must not break the page.
        private static TeamRegistrationViewModel WithAllPlayerFields(TeamRegistrationViewModel model)
        {
            while (model.Players.Count < MaxPlayersPerTeam)
            {
                model.Players.Add(new PlayerViewModel());
            }

            return model;
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
                TempData["ErrorMessage"] = localizer.ServiceMessage(error.Message);
                return RedirectToAction("CityTournaments", "Tournament", new { id = cityId });
            }

            if (!ModelState.IsValid)
            {
                // The form's own checks (TeamRegistrationViewModel.Validate) speak English like the services:
                // translate them the same way.
                if (ModelState.TryGetValue(string.Empty, out var formErrors))
                {
                    var texts = formErrors.Errors.Select(e => localizer.KnownServiceMessage(e.ErrorMessage) ?? e.ErrorMessage).ToList();
                    formErrors.Errors.Clear();
                    texts.ForEach(formErrors.Errors.Add);
                }

                return View(WithAllPlayerFields(viewModel));
            }

            int teamId;
            try
            {
                teamId = await teamService.CreateTeamDraftAsync(viewModel, userId);
            }
            catch (InvalidOperationException ex) when (localizer.KnownServiceMessage(ex.Message) is string message)
            {
                // Only the service's own validation messages ("A Team with the same name already exists" ...)
                // are shown; any other error (e.g. a technical one from EF) goes to the general catch below.
                ModelState.AddModelError("", message);
                return View(WithAllPlayerFields(viewModel));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Creating a team failed for user {UserId}", userId);
                ModelState.AddModelError("", localizer["GenericErrorMessage"].Value);
                return View(WithAllPlayerFields(viewModel));
            }

            return RedirectToAction("Declaration", new
            {
                tournamentId = id,
                teamId = teamId
            });
        }

        [HttpGet]
        public async Task<IActionResult> Declaration(int tournamentId, int teamId)
        {
            var userId = User.Id();

            var error = await teamService.CheckForErrorsAsync(tournamentId, userId);
            if (error != null)
            {
                if (error.Message == "You need to become a coach to join a team.")
                {
                    return RedirectToAction("BecomeCoach", "Coach", new { tournamentId });
                }

                var cityId = await teamService.GetCityIdAsync(tournamentId);
                TempData["ErrorMessage"] = localizer.ServiceMessage(error.Message);
                return RedirectToAction("CityTournaments", "Tournament", new { id = cityId });
            }

            // Only the coach's own team can be registered; ignore any other id in the URL.
            var ownTeamId = await teamService.GetCoachTeamIdAsync(userId);
            if (!ownTeamId.HasValue)
            {
                return RedirectToAction(nameof(JoinTeam), new { id = tournamentId });
            }

            var model = new TournamentDeclarationViewModel
            {
                TournamentId = tournamentId,
                TeamId = ownTeamId.Value,
                DeclarationText = localizer["TournamentDeclaration"].Value
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Declaration(TournamentDeclarationViewModel model)
        {
            var userId = User.Id();

            model.DeclarationText = localizer["TournamentDeclaration"].Value;

            if (!model.AcceptLiabilityDeclaration)
            {
                ModelState.AddModelError(nameof(model.AcceptLiabilityDeclaration), localizer["SMDeclarationRequired"].Value);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Re-run every join rule on POST: the form can be submitted directly, bypassing the GET checks.
            var error = await teamService.CheckForErrorsAsync(model.TournamentId, userId);
            if (error != null)
            {
                if (error.Message == "You need to become a coach to join a team.")
                {
                    return RedirectToAction("BecomeCoach", "Coach", new { tournamentId = model.TournamentId });
                }

                var cityId = await teamService.GetCityIdAsync(model.TournamentId);
                TempData["ErrorMessage"] = localizer.ServiceMessage(error.Message);
                return RedirectToAction("CityTournaments", "Tournament", new { id = cityId });
            }

            // Never trust the TeamId from the form: always register the coach's own team.
            var ownTeamId = await teamService.GetCoachTeamIdAsync(userId);
            if (!ownTeamId.HasValue)
            {
                return RedirectToAction(nameof(JoinTeam), new { id = model.TournamentId });
            }

            // Left this tournament earlier and the fee is still with the organiser: it pays for the place again.
            var paidOrderId = await paymentService.ReusePaidOrderAsync(model.TournamentId, userId, ownTeamId.Value);
            if (paidOrderId.HasValue)
            {
                var joinCityId = await teamService.GetCityIdAsync(model.TournamentId);

                try
                {
                    await teamService.FinalizeJoinAsync(model.TournamentId, userId, ownTeamId.Value, joinCityId);
                }
                catch (InvalidOperationException ex) when (localizer.KnownServiceMessage(ex.Message) is string message)
                {
                    TempData["ErrorMessage"] = message;
                    return RedirectToAction("CityTournaments", "Tournament", new { id = joinCityId });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Joining tournament {TournamentId} with an already paid fee failed", model.TournamentId);
                    TempData["ErrorMessage"] = localizer["GenericErrorMessage"].Value;
                    return RedirectToAction("CityTournaments", "Tournament", new { id = joinCityId });
                }

                TempData["SuccessMessage"] = localizer["RejoinedWithPaidFee"].Value;
                return RedirectToAction("CityTournaments", "Tournament", new { id = joinCityId });
            }

            string url;
            try
            {
                url = await paymentService.CreateTournamentJoinCheckoutAsync(
                    model.TournamentId,
                    userId,
                    ownTeamId.Value,
                    model.AcceptLiabilityDeclaration,
                    model.DeclarationText);
            }
            catch (InvalidOperationException ex) when (localizer.KnownServiceMessage(ex.Message) is string message)
            {
                var cityId = await teamService.GetCityIdAsync(model.TournamentId);
                TempData["ErrorMessage"] = message;
                return RedirectToAction("CityTournaments", "Tournament", new { id = cityId });
            }
            catch (Exception ex)
            {
                // Stripe errors and anything else technical: logged, and a friendly message for the coach.
                logger.LogError(ex, "Creating Stripe checkout failed for tournament {TournamentId}", model.TournamentId);
                var cityId = await teamService.GetCityIdAsync(model.TournamentId);
                TempData["ErrorMessage"] = localizer["PaymentStartError"].Value;
                return RedirectToAction("CityTournaments", "Tournament", new { id = cityId });
            }

            return Redirect(url);
        }

    }
}

using FootballProjectSoftUni.Core.Contracts.City;
using FootballProjectSoftUni.Core.Contracts.Tournament;
using FootballProjectSoftUni.Core.Contracts.TournamentJoinPayment;
using FootballProjectSoftUni.Core.Models.City;
using FootballProjectSoftUni.Core.Models.Tournament;
using FootballProjectSoftUni.Core.Services.Gallery;
using FootballProjectSoftUni.Extensions;
using FootballProjectSoftUni.Services;
using FootballProjectSoftUni.Infrastructure.Data;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using FootballProjectSoftUni.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace FootballProjectSoftUni.Controllers
{
    [Authorize]
    public class CityController : Controller
    {
        private readonly ICityService cityService;
        private readonly ITournamentJoinPaymentService tournamentJoinPaymentService;
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly ITournamentDeletionService deletionService;
        private readonly TournamentDeletionFollowUp followUp;
        private readonly CoverPictureUpload pictures;
        private readonly ILogger<CityController> logger;

        public CityController(
            ICityService _service,
            ITournamentJoinPaymentService _tournamentJoinPaymentService,
            IStringLocalizer<SharedResource> _localizer,
            ITournamentDeletionService _deletionService,
            TournamentDeletionFollowUp _followUp,
            CoverPictureUpload _pictures,
            ILogger<CityController> _logger)
        {
            tournamentJoinPaymentService = _tournamentJoinPaymentService;
            cityService = _service;
            localizer = _localizer;
            deletionService = _deletionService;
            followUp = _followUp;
            pictures = _pictures;
            logger = _logger;
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
        public async Task<IActionResult> AddCity(CityViewModel model, IFormFile? image)
        {
            if (User.IsAdmin() == false)
            {
                return Unauthorized();
            }

            await pictures.CheckAsync(image, ModelState, required: true);

            if (ModelState.IsValid == false)
            {
                pictures.AskAgainIfChosen(image, ModelState);
                return View(model);
            }

            var imageUrl = await pictures.SaveAsync(image!, CoverKind.Cities, ModelState);
            if (imageUrl == null)
            {
                return View(model);
            }

            model.ImageUrl = imageUrl;

            try
            {
                await cityService.AddCityAsync(model);
            }
            catch
            {
                pictures.Delete(imageUrl);
                throw;
            }

            return RedirectToAction(nameof(All));
        }

        // A new picture for a city; the old one is removed from the site.
        [HttpGet]
        public async Task<IActionResult> ChangePicture(int? id)
        {
            if (User.IsAdmin() == false)
            {
                return Unauthorized();
            }

            return View(await ChangePictureModelAsync(id));
        }

        [HttpPost]
        public async Task<IActionResult> ChangePicture(int? id, IFormFile? image)
        {
            if (User.IsAdmin() == false)
            {
                return Unauthorized();
            }

            var model = await ChangePictureModelAsync(id);

            if (model.Items.All(c => c.Id != id))
            {
                ModelState.AddModelError("id", localizer["PicPickCity"].Value);
            }

            await pictures.CheckAsync(image, ModelState, required: true);

            if (ModelState.IsValid == false)
            {
                pictures.AskAgainIfChosen(image, ModelState);
                return View(model);
            }

            var imageUrl = await pictures.SaveAsync(image!, CoverKind.Cities, ModelState);
            if (imageUrl == null)
            {
                return View(model);
            }

            string? oldImageUrl;
            try
            {
                oldImageUrl = await cityService.ChangeCityImageAsync(id!.Value, imageUrl);
            }
            catch
            {
                pictures.Delete(imageUrl);
                throw;
            }

            // Deleted in the meantime.
            if (oldImageUrl == null)
            {
                pictures.Delete(imageUrl);
                return NotFound();
            }

            pictures.Delete(oldImageUrl);

            TempData["PaymentMessage"] = localizer["PicChanged"].Value;
            TempData["PaymentType"] = "success";

            return RedirectToAction(nameof(All));
        }

        private async Task<ChangePictureViewModel> ChangePictureModelAsync(int? id) => new()
        {
            Controller = "City",
            SelectedId = id,
            Items = (await cityService.AllCitiesAsync())
                .OrderBy(c => c.Name)
                .Select(c => new PictureOwner(c.Id, c.Name, c.ImageUrl))
                .ToList()
        };

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

            // What the deletion will do: refunds for paid teams, or why it is not possible yet.
            town.Deletion = await deletionService.PreviewCityAsync(id);

            return View(town);

        }

        [HttpPost]

        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (User.IsAdmin() == false)
            {
                return Unauthorized();
            }

            TournamentDeletionResult result;
            try
            {
                result = await deletionService.DeleteCityAsync(id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Deleting city {CityId} failed", id);
                TempData["ErrorMessage"] = localizer["GenericErrorMessage"].Value;
                return RedirectToAction(nameof(DeleteConfirmed), new { id });
            }

            await followUp.RunAsync(result);

            switch (result.Status)
            {
                case TournamentDeletionStatus.NotFound:
                    return NotFound();

                case TournamentDeletionStatus.Running:
                    TempData["ErrorMessage"] = localizer["DCRunningBlocked", result.RunningUntil!.Value.ToString("dd.MM.yyyy HH:mm")].Value;
                    return RedirectToAction(nameof(DeleteConfirmed), new { id });

                case TournamentDeletionStatus.RefundsFailed:
                    TempData["ErrorMessage"] = localizer["DCRefundsFailed", string.Join(", ", result.FailedTeams)].Value;
                    return RedirectToAction(nameof(DeleteConfirmed), new { id });

                case TournamentDeletionStatus.DeleteFailed:
                    logger.LogError(result.Error, "Deleting city {CityId} failed", id);
                    TempData["ErrorMessage"] = localizer["DCDeleteFailed"].Value;
                    return RedirectToAction(nameof(DeleteConfirmed), new { id });
            }

            TempData["PaymentMessage"] = result.Refunded.Any()
                ? localizer["DCDeletedRefunded", result.Refunded.Count].Value
                : localizer["DCDeleted"].Value;
            TempData["PaymentType"] = "success";

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

            // The ranking is public, the coaches' names are not: guests see only the teams.
            if (User.Identity?.IsAuthenticated != true)
            {
                bestTeams.ForEach(t => t.CoachName = string.Empty);
            }

            return View(bestTeams);
        }

        [Authorize]
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

            var form = await cityService.GetUpdateCityBestTeamFormAsync();

            // Only a city and a team that exist (anything else comes from an edited form and used to end with an error page).
            if (!form.Cities.Any(c => c.Id == model.CityId))
            {
                ModelState.AddModelError(nameof(model.CityId), localizer["MBInvalid"].Value);
            }

            if (!form.Teams.Any(t => t.Id == model.TeamId))
            {
                ModelState.AddModelError(nameof(model.TeamId), localizer["MBInvalid"].Value);
            }

            if (!ModelState.IsValid)
            {
                model.Cities = form.Cities;
                model.Teams = form.Teams;

                return View(model);
            }

            await cityService.IncrementTeamWinsInCityAsync(model.CityId, model.TeamId);

            return RedirectToAction("GetBestTeams", new { id = model.CityId });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> PaymentSuccess(int orderId)
        {
            var order = await tournamentJoinPaymentService.GetTournametJoinPaymentOrder(orderId);

            // Order numbers go one after another, so the state of an order is shown only to the person who paid it
            // (anybody else - or somebody whose session ran out during the payment - gets a neutral message).
            if (order == null || User.Id() == null || order.UserId != User.Id())
            {
                TempData["PaymentMessage"] = localizer["PaymentCheckEmail"].Value;
                TempData["PaymentType"] = "info";
                return RedirectToAction(nameof(All));
            }

            if (order.Status == "Paid")
            {
                TempData["PaymentMessage"] = localizer["PaymentSuccessMessage"].Value;
                TempData["PaymentType"] = "success";
            }
            else
            {
                // Stripe's confirmation hasn't arrived yet: the page asks PaymentStatus every few seconds.
                TempData["PaymentMessage"] = localizer["PaymentPendingMessage"].Value;
                TempData["PaymentType"] = "info";
                TempData["PaymentPendingOrder"] = orderId;
            }

            return RedirectToAction(nameof(All));
        }

        /// <summary>Whether the signed-in user's order is paid yet (asked by the page after the payment).</summary>
        [HttpGet]
        public async Task<IActionResult> PaymentStatus(int orderId)
        {
            var order = await tournamentJoinPaymentService.GetTournametJoinPaymentOrder(orderId);

            // Only the person who paid learns the state of an order.
            if (order == null || order.UserId != User.Id())
            {
                return NotFound();
            }

            return Json(new { paid = order.Status == "Paid" });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult PaymentCancel(int orderId)
        {
            TempData["PaymentMessage"] = localizer["PaymentCanceledMessage"].Value;
            TempData["PaymentType"] = "danger";
            return RedirectToAction(nameof(All));
        }
    }

}


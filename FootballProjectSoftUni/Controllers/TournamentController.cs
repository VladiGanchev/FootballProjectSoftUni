using FootballProjectSoftUni.Core.Contracts.Notification;
using FootballProjectSoftUni.Core.Contracts.Tournament;
using FootballProjectSoftUni.Core.Extensions;
using FootballProjectSoftUni.Core.Models.City;
using FootballProjectSoftUni.Core.Models.Notification;
using FootballProjectSoftUni.Core.Models.Settings;
using FootballProjectSoftUni.Core.Models.Tournament;
using FootballProjectSoftUni.Core.Services.Gallery;
using FootballProjectSoftUni.Core.Services.Tournament;
using FootballProjectSoftUni.Extensions;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using FootballProjectSoftUni.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net;
using static FootballProjectSoftUni.Infrastructure.Data.Constants.DataConstants;

namespace FootballProjectSoftUni.Controllers
{
    [Authorize]
    public class TournamentController : Controller
    {
        private readonly ITournamentService service;
        private readonly INotificationService notificationService;
        private readonly ITournamentDeletionService deletionService;
        private readonly TournamentDeletionFollowUp followUp;
        private readonly CoverPictureUpload pictures;
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly ILogger<TournamentController> logger;
        private readonly SiteSettings site;

        public TournamentController(
            ITournamentService _service,
            INotificationService _notificationService,
            ITournamentDeletionService _deletionService,
            TournamentDeletionFollowUp _followUp,
            CoverPictureUpload _pictures,
            IStringLocalizer<SharedResource> _localizer,
            ILogger<TournamentController> _logger,
            IOptions<SiteSettings> _siteOptions)
        {
            service = _service;
            notificationService = _notificationService;
            deletionService = _deletionService;
            followUp = _followUp;
            pictures = _pictures;
            localizer = _localizer;
            logger = _logger;
            site = _siteOptions.Value;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> CityTournaments(int id, bool showPast, int? page)
        {
            var city = await service.FindCityAsync(id);

            if (city == null)
            {
                return BadRequest();
            }

            var tournaments = await service.GetCityTournamentsAsync(id, showPast, page);

            ViewBag.ShowPast = showPast;
            ViewBag.CityId = id;
            ViewBag.CityName = city.Name;

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

        public async Task<IActionResult> AddTournamentToCity(AddTournamentFormViewModel model, int cityId, IFormFile? image)
        {
            if (User.IsAdmin() == false)
            {
                return Unauthorized();
            }

            ReadDates(model.StartDate, model.EndDate, out var start, out var end);
            await pictures.CheckAsync(image, ModelState, required: true);

            if (!ModelState.IsValid)
            {
                pictures.AskAgainIfChosen(image, ModelState);
                model.Cities = await service.GetCitiesAsync();
                return View(model);
            }

            var imageUrl = await pictures.SaveAsync(image!, CoverKind.Tournaments, ModelState);
            if (imageUrl == null)
            {
                model.Cities = await service.GetCitiesAsync();
                return View(model);
            }

            model.ImageUrl = imageUrl;

            try
            {
                await service.AddTournamentToCityAsync(model, cityId, start, end);
            }
            catch
            {
                pictures.Delete(imageUrl);
                throw;
            }

            // The tournament is already saved: a failed announcement must not show an error page
            // (the organiser would create the tournament a second time).
            try
            {
                var city = await service.FindCityAsync(cityId);
                if (city != null)
                {
                    var link = site.Absolute(Url.Action(nameof(CityTournaments), new { id = cityId })!, Request);
                    var unsubscribe = site.Absolute(Url.Action("Unsubscribe", "EmailPreferences")!, Request);
                    await notificationService.CreateNotificationForCityCoachesAsync(cityId,
                        culture => NewTournamentText(culture, city.Name, start, link), unsubscribe);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "The new tournament in city {CityId} was not announced to the coaches", cityId);
            }

            return RedirectToAction("All", "City");
        }

        private AnnouncementText NewTournamentText(string culture, string city, DateTime start, string link) =>
            CultureScope.Run(culture, () =>
            {
                var date = start.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);

                return new AnnouncementText(
                    localizer["NewTournamentSubject", city].Value,
                    localizer["NewTournamentNotification", city, date].Value,
                    localizer["NewTournamentEmailBody", WebUtility.HtmlEncode(city), date, WebUtility.HtmlEncode(link),
                        AnnouncementText.UnsubscribeLinkPlaceholder].Value);
            });

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
                return NotFound();
            }

            return View(tournament);
        }

        [HttpPost]

        public async Task<IActionResult> Edit(EditViewModel model, IFormFile? image)
        {
            if (User.IsAdmin() == false)
            {
                return Unauthorized();
            }

            var tournament = await service.FindTournamentByIdAsync(model.Id);

            if (tournament == null)
            {
                return NotFound();
            }

            // The picture is not a field of the form: the current one stays unless a new file is chosen.
            model.ImageUrl = tournament.ImageUrl;
            await pictures.CheckAsync(image, ModelState, required: false);

            // Once the tournament has started its dates stay as they are, whatever the form sends.
            model.DatesLocked = TournamentService.DatesLocked(tournament);
            var start = tournament.StartDate;
            var end = tournament.EndDate;

            if (model.DatesLocked)
            {
                ModelState.Remove(nameof(model.StartDate));
                ModelState.Remove(nameof(model.EndDate));
                model.StartDate = start.ToDateTimeInput();
                model.EndDate = end.ToDateTimeInput();
            }
            else
            {
                ReadDates(model.StartDate, model.EndDate, out start, out end);

                // The form has minute precision: a date that wasn't touched keeps its stored value (seconds included)
                // and doesn't count as a new date.
                start = KeepIfSameMinute(start, tournament.StartDate);
                end = KeepIfSameMinute(end, tournament.EndDate);
            }

            // Never save a form that was not read completely (a prize that could not be read used to be saved as 0).
            if (!ModelState.IsValid)
            {
                pictures.AskAgainIfChosen(image, ModelState);

                // The winner comes from the final, not from this form.
                model.Winner = tournament.Winner;
                return View(model);
            }

            var oldStart = tournament.StartDate;
            var oldImageUrl = tournament.ImageUrl;

            if (image != null)
            {
                var imageUrl = await pictures.SaveAsync(image, CoverKind.Tournaments, ModelState);
                if (imageUrl == null)
                {
                    model.Winner = tournament.Winner;
                    return View(model);
                }

                model.ImageUrl = imageUrl;
            }

            try
            {
                await service.EditTournamentAsync(model, start, end);
            }
            catch
            {
                if (model.ImageUrl != oldImageUrl)
                {
                    pictures.Delete(model.ImageUrl);
                }

                throw;
            }

            if (model.ImageUrl != oldImageUrl)
            {
                pictures.Delete(oldImageUrl);
            }

            var moved = start != oldStart;
            if (moved)
            {
                await AnnounceNewDateAsync(tournament, oldStart, start);
            }

            TempData["SuccessMessage"] = localizer[moved ? "TEditSavedMoved" : "TEditSaved"].Value;

            var cityId = tournament.TournamentCities.FirstOrDefault()?.CityId;

            return cityId.HasValue
                ? RedirectToAction(nameof(CityTournaments), new { id = cityId.Value })
                : RedirectToAction("All", "City");
        }

        private static DateTime KeepIfSameMinute(DateTime posted, DateTime stored) =>
            posted == stored.AddTicks(-(stored.Ticks % TimeSpan.TicksPerMinute)) ? stored : posted;

        // The coaches and the referee learn about the new date. The change is already saved,
        // so a failure here must not show an error page.
        private async Task AnnounceNewDateAsync(FootballProjectSoftUni.Infrastructure.Data.Models.Tournament tournament, DateTime oldStart, DateTime newStart)
        {
            try
            {
                var city = tournament.TournamentCities.FirstOrDefault()?.City;
                var link = site.Absolute(city != null ? Url.Action(nameof(CityTournaments), new { id = city.Id })! : "/", Request);

                await notificationService.CreateNotificationForTournamentParticipantsAsync(tournament.Id,
                    culture => TournamentMovedText(culture, city?.Name, oldStart, newStart, link));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "The participants of tournament {TournamentId} were not told about its new date", tournament.Id);
            }
        }

        private AnnouncementText TournamentMovedText(string culture, string? city, DateTime oldStart, DateTime newStart, string link) =>
            CultureScope.Run(culture, () =>
            {
                var cityName = string.IsNullOrWhiteSpace(city) ? localizer["ReminderUnknownCity"].Value : city;
                var from = oldStart.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
                var to = newStart.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);

                return new AnnouncementText(
                    localizer["TournamentMovedSubject"].Value,
                    localizer["TournamentMovedNotification", cityName, from, to].Value,
                    localizer["TournamentMovedEmailBody", WebUtility.HtmlEncode(cityName), from, to, WebUtility.HtmlEncode(link)].Value);
            });

        /// <summary>
        /// Reads the start / end date pickers. Adds a form error for a missing or invalid date
        /// and for a tournament that would end before it starts.
        /// </summary>
        private void ReadDates(string startText, string endText, out DateTime start, out DateTime end)
        {
            var startOk = FormDates.TryParseDateTime(startText, out start);
            var endOk = FormDates.TryParseDateTime(endText, out end);

            if (!startOk)
            {
                ModelState.AddModelError("StartDate", localizer["TDInvalidDate"].Value);
            }

            if (!endOk)
            {
                ModelState.AddModelError("EndDate", localizer["TDInvalidDate"].Value);
            }

            // A start in the past would be picked up within minutes by the start job (started, or cancelled
            // with refunds when there are too few teams).
            if (startOk && start <= BgTime.Now)
            {
                ModelState.AddModelError("StartDate", localizer["TDStartInPast"].Value);
            }

            if (startOk && endOk && end <= start)
            {
                ModelState.AddModelError("EndDate", localizer["TDEndBeforeStart"].Value);
            }
        }

        // Confirmation page: shows what will happen (refunds, or why the tournament cannot be deleted yet).
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (User.IsAdmin() == false)
            {
                return Unauthorized();
            }

            var preview = await deletionService.PreviewTournamentAsync(id);

            if (preview == null)
            {
                return NotFound();
            }

            return View(preview);
        }

        [HttpPost]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (User.IsAdmin() == false)
            {
                return Unauthorized();
            }

            TournamentDeletionResult result;
            try
            {
                result = await deletionService.DeleteTournamentAsync(id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Deleting tournament {TournamentId} failed", id);
                TempData["ErrorMessage"] = localizer["GenericErrorMessage"].Value;
                return RedirectToAction(nameof(Delete), new { id });
            }

            await followUp.RunAsync(result);

            switch (result.Status)
            {
                case TournamentDeletionStatus.NotFound:
                    return NotFound();

                case TournamentDeletionStatus.Running:
                    TempData["ErrorMessage"] = localizer["TDRunningBlocked", result.RunningUntil!.Value.ToString("dd.MM.yyyy HH:mm")].Value;
                    return RedirectToAction(nameof(Delete), new { id });

                case TournamentDeletionStatus.RefundsFailed:
                    TempData["ErrorMessage"] = localizer["TDRefundsFailed", string.Join(", ", result.FailedTeams)].Value;
                    return RedirectToAction(nameof(Delete), new { id });

                case TournamentDeletionStatus.DeleteFailed:
                    logger.LogError(result.Error, "Deleting tournament {TournamentId} failed", id);
                    TempData["ErrorMessage"] = localizer["TDDeleteFailed"].Value;
                    return RedirectToAction(nameof(Delete), new { id });
            }

            TempData["SuccessMessage"] = result.Refunded.Any()
                ? localizer["TDDeletedRefunded", result.Refunded.Count].Value
                : localizer["TDDeleted"].Value;

            return result.CityId.HasValue
                ? RedirectToAction(nameof(CityTournaments), new { id = result.CityId.Value })
                : RedirectToAction("All", "City");
        }
    }
}

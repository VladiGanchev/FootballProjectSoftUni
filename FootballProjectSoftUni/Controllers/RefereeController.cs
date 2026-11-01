using FootballProjectSoftUni.Core.Contracts.Email;
using FootballProjectSoftUni.Core.Contracts.Message;
using FootballProjectSoftUni.Core.Contracts.Notification;
using FootballProjectSoftUni.Core.Contracts.Referee;
using FootballProjectSoftUni.Core.Contracts.Tournament;
using FootballProjectSoftUni.Core.Extensions;
using FootballProjectSoftUni.Core.Models.Referee;
using FootballProjectSoftUni.Extensions;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using System.Net;
using System.Security.Claims;
using static FootballProjectSoftUni.Infrastructure.Data.Constants.DataConstants;

namespace FootballProjectSoftUni.Controllers
{
    [Authorize]
    public class RefereeController : Controller
    {
        private readonly IRefereeService refereeService;
        private readonly ITournamentService tournamentService;
        private readonly INotificationService notificationService;
        private readonly IContactMessageService contactMessageService;
        private readonly IEmailQueue emailQueue;

        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly ILogger<RefereeController> logger;

        public RefereeController(IRefereeService _refereeService, ITournamentService _tournamentService, INotificationService _notificationService, IContactMessageService _contactMessageService, IEmailQueue _emailQueue, IStringLocalizer<SharedResource> _localizer, ILogger<RefereeController> _logger)
        {
            localizer = _localizer;
            logger = _logger;
            refereeService = _refereeService;
            tournamentService = _tournamentService;
            notificationService = _notificationService;
            contactMessageService = _contactMessageService;
            emailQueue = _emailQueue;
        }

        [HttpGet]
        public async Task<IActionResult> BecomeReferee(int id)
        {
            string userId = User.Id();

            // A wrong address (no such tournament) shows the "not found" page.
            if (await tournamentService.FindTournamentByIdAsync(id) == null)
            {
                return NotFound();
            }

            if (await HasStartedAsync(id))
            {
                TempData["ErrorMessage"] = localizer["SMRefereeTournamentStarted"].Value;
                return await RedirectToTournamentCity(id);
            }

            var result = await refereeService.CheckForErrorsAsync(id, userId);

            if (result != null)
            {
                TempData["ErrorMessage"] = localizer.ServiceMessage(result.Message);
                return await RedirectToTournamentCity(id);
            }

            var alreadyReferee = await refereeService.GetRefereeByUserIdAsync(userId);

            if (alreadyReferee != null)
            {
                // Existing referees confirm with a POST (a GET must never change data).
                ViewBag.TournamentId = id;
                return View("ConfirmReferee", alreadyReferee.Name);
            }

            var model = new RefereeFormViewMOdel
            {
                TournamentId = id
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> AssignExistingReferee(int id)
        {
            var userId = User.Id();

            if (await HasStartedAsync(id))
            {
                TempData["ErrorMessage"] = localizer["SMRefereeTournamentStarted"].Value;
                return await RedirectToTournamentCity(id);
            }

            var error = await refereeService.CheckForErrorsAsync(id, userId);
            if (error != null)
            {
                TempData["ErrorMessage"] = localizer.ServiceMessage(error.Message);
                return await RedirectToTournamentCity(id);
            }

            var success = await refereeService.AssignExistingRefereeToTournamentAsync(userId, id);
            if (!success)
            {
                return BadRequest();
            }

            return RedirectToAction(nameof(AllTournamentsToReferee));
        }

        private async Task<bool> HasStartedAsync(int tournamentId)
        {
            var tournament = await tournamentService.FindTournamentByIdAsync(tournamentId);
            return tournament != null && BgTime.Now >= tournament.StartDate;
        }

        private async Task<IActionResult> RedirectToTournamentCity(int tournamentId)
        {
            var tournament = await tournamentService.FindTournamentByIdAsync(tournamentId);
            var cityId = tournament?.TournamentCities?.FirstOrDefault()?.CityId;

            return cityId.HasValue
                ? RedirectToAction("CityTournaments", "Tournament", new { id = cityId.Value })
                : RedirectToAction("All", "City");
        }

        [HttpPost]
        public async Task<IActionResult> BecomeReferee(RefereeFormViewMOdel model, int id)
        {
            // Same rules as the GET: the form can be posted directly for any tournament.
            if (await HasStartedAsync(id))
            {
                TempData["ErrorMessage"] = localizer["SMRefereeTournamentStarted"].Value;
                return await RedirectToTournamentCity(id);
            }

            var checkError = await refereeService.CheckForErrorsAsync(id, User.Id());
            if (checkError != null)
            {
                TempData["ErrorMessage"] = localizer.ServiceMessage(checkError.Message);
                return await RedirectToTournamentCity(id);
            }

            // The date picker sends "1990-04-15" in every language (older "15.04.1990" style values work too).
            if (!FormDates.TryParseDate(model.Birthdate, out var birthdate))
            {
                ModelState.AddModelError(nameof(model.Birthdate), localizer["BRInvalidBirthdate"].Value);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int age = CalculateAge(birthdate);

            if (age < 18)
            {
                ModelState.AddModelError(nameof(model.Birthdate), localizer["SMMinAge18"].Value);
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

        [HttpPost]
        public async Task<IActionResult> LeaveTournament(int id)
        {
            var userId = User.Id();

            var result = await refereeService.LeaveTournamentAsync(id, userId);

            if (result == false)
            {
                // Started already (leaving is possible only before the start) or not its referee.
                TempData["ErrorMessage"] = localizer["RefereeLeaveNotAllowed"].Value;
                return RedirectToAction(nameof(AllTournamentsToReferee));
            }

            return await RedirectToTournamentCity(id);
        }
        public static int CalculateAge(DateTime birthdate)
        {
            DateTime today = BgTime.Today;
            int age = today.Year - birthdate.Year;
            if (birthdate.Date > today.AddYears(-age))
            {
                age--; 
            }
            return age;
        }

        [HttpGet]
        public async Task<IActionResult> Referees()
        {
            var userId = User.Id();
            var model = await refereeService.GetAllRefereesWithRatingsAsync(userId);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Rate(string refereeId, int rating)
        {
            var userId = User.Id();

            if (rating < 1 || rating > 5)
            {
                TempData["ErrorMessage"] = localizer["SMRatingRange"].Value;
                return RedirectToAction(nameof(Referees));
            }

            if (refereeId == userId)
            {
                TempData["ErrorMessage"] = localizer["RateSelfNotAllowed"].Value;
                return RedirectToAction(nameof(Referees));
            }

            try
            {
                await refereeService.RateRefereeAsync(refereeId, userId, rating);
            }
            catch (ArgumentException)
            {
                TempData["ErrorMessage"] = localizer["RefereeNotFound"].Value;
            }

            return RedirectToAction(nameof(Referees));
        }

        [HttpGet]
        public async Task<IActionResult> Invite()
        {
            if (!User.IsAdmin())
            {
                return Unauthorized();
            }

            var referees = await refereeService.GetAllRefereesWithRatingsAsync(User.Id());

            var model = new RefereeInviteFormViewModel
            {
                Referees = referees.Select(r => new SelectListItem
                {
                    Value = r.Id,
                    Text = r.Name
                })
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Invite(RefereeInviteFormViewModel model)
        {
            if (!User.IsAdmin())
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(model.RefereeId))
            {
                ModelState.AddModelError(nameof(model.RefereeId), localizer["SMChooseReferee"].Value);
            }

            if (string.IsNullOrWhiteSpace(model.Subject))
            {
                ModelState.AddModelError(nameof(model.Subject), localizer["SMEnterSubject"].Value);
            }

            if (string.IsNullOrWhiteSpace(model.Content))
            {
                ModelState.AddModelError(nameof(model.Content), localizer["SMEnterMessage"].Value);
            }

            // Checked before anything is saved (an unknown user can't get the message).
            string? email = null;
            if (!string.IsNullOrWhiteSpace(model.RefereeId))
            {
                try
                {
                    email = await refereeService.GetRefereeEmail(model.RefereeId);
                }
                catch (ArgumentException)
                {
                    ModelState.AddModelError(nameof(model.RefereeId), localizer["RefereeNotFound"].Value);
                }
            }

            if (!ModelState.IsValid)
            {
                var referees = await refereeService.GetAllRefereesWithRatingsAsync(User.Id());
                model.Referees = referees.Select(r => new SelectListItem
                {
                    Value = r.Id,
                    Text = r.Name
                });

                return View(model);
            }

            await contactMessageService.SendInitialAsync(
                User.Id(),           
                model.Subject,
                model.Content,
                model.RefereeId       
            );

            // Sent in the background. The text is shown exactly as typed (line breaks kept, no HTML);
            // an e-mail subject can't contain line breaks.
            emailQueue.Enqueue(
                email!,
                model.Subject.ReplaceLineEndings(" "),
                "<p>" + WebUtility.HtmlEncode(model.Content).ReplaceLineEndings("<br>") + "</p>");

            TempData["Success"] = localizer["SMInviteSent"].Value;
            return RedirectToAction(nameof(Referees));
        }

        [HttpGet]
        public async Task<IActionResult> Comments(string id)
        {
            try
            {
                var model = await refereeService.GetCommentsAsync(id);
                return View(model);
            }
            catch (ArgumentException)
            {
                // Unknown id or a profile that was switched off.
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(string refereeId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = localizer["SMCommentEmpty"].Value;
                return RedirectToAction(nameof(Comments), new { id = refereeId });
            }

            content = content.Trim();
            if (content.Length > RefereeCommentMaxLength)
            {
                TempData["Error"] = localizer["SMCommentTooLong", RefereeCommentMaxLength].Value;
                return RedirectToAction(nameof(Comments), new { id = refereeId });
            }

            if (string.IsNullOrWhiteSpace(refereeId) || await refereeService.GetRefereeByUserIdAsync(refereeId) == null)
            {
                return NotFound();
            }

            if (refereeId == User.Id())
            {
                TempData["Error"] = localizer["CommentSelfNotAllowed"].Value;
                return RedirectToAction(nameof(Comments), new { id = refereeId });
            }

            if (!await refereeService.CanCommentAsync(User.Id()))
            {
                TempData["Error"] = localizer["SMCommentWait"].Value;
                return RedirectToAction(nameof(Comments), new { id = refereeId });
            }

            await refereeService.AddCommentAsync(refereeId, User.Id(), content);

            return RedirectToAction(nameof(Comments), new { id = refereeId });
        }

        // Anybody who reads the comments can report one that breaks the rules (insults, defamation, somebody's
        // personal data...). The report goes to the organisers' inbox, where they answer it.
        [HttpGet]
        public async Task<IActionResult> ReportComment(int id)
        {
            var comment = await refereeService.GetCommentAsync(id);

            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        [HttpPost]
        public async Task<IActionResult> ReportComment(int id, string? reason)
        {
            var comment = await refereeService.GetCommentAsync(id);

            if (comment == null)
            {
                return NotFound();
            }

            reason = reason?.Trim();
            if (reason?.Length > RefereeCommentMaxLength)
            {
                reason = reason.Substring(0, RefereeCommentMaxLength);
            }

            var at = BgTime.FromUtc(comment.CreatedOn).ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);

            await contactMessageService.SendInitialAsync(
                User.Id(),
                localizer["CRReportSubject", comment.RefereeName].Value,
                localizer["CRReportBody", comment.Id, comment.RefereeName, comment.UserName, at, comment.Content,
                    string.IsNullOrWhiteSpace(reason) ? "-" : reason].Value);

            TempData["Success"] = localizer["CRReportSent"].Value;
            return RedirectToAction(nameof(Comments), new { id = comment.RefereeId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComment(int id)
        {
            if (!User.IsAdmin())
            {
                return Unauthorized();
            }

            var comment = await refereeService.DeleteCommentAsync(id);

            if (comment == null)
            {
                return NotFound();
            }

            // The author learns that the comment was removed and why (EU Digital Services Act).
            // The comment is already gone, so a failure here must not show an error page.
            try
            {
                var at = BgTime.FromUtc(comment.CreatedOn).ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
                await notificationService.CreateNotificationForUserAsync(comment.UserId,
                    CultureScope.Run("bg", () => localizer["CRRemovedNotification", comment.RefereeName, at].Value));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "The author of removed comment {CommentId} was not notified", id);
            }

            TempData["Success"] = localizer["CRDeleted"].Value;
            return RedirectToAction(nameof(Comments), new { id = comment.RefereeId });
        }


    }
}

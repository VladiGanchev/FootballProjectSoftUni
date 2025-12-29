using FootballProjectSoftUni.Core.Contracts.Message;
using FootballProjectSoftUni.Core.Contracts.Notification;
using FootballProjectSoftUni.Core.Contracts.Referee;
using FootballProjectSoftUni.Core.Contracts.Tournament;
using FootballProjectSoftUni.Core.Models.Referee;
using FootballProjectSoftUni.Extensions;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        private readonly INotificationService notificationService;
        private readonly IContactMessageService contactMessageService;

        public RefereeController(IRefereeService _refereeService, ITournamentService _tournamentService, INotificationService _notificationService, IContactMessageService _contactMessageService)
        {
            refereeService = _refereeService;
            tournamentService = _tournamentService;
            notificationService = _notificationService;
            contactMessageService = _contactMessageService;
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

            // 🔹 НОВО: ако вече има Referee запис за този user, НЕ показваме форма
            var alreadyReferee = await refereeService.GetRefereeByUserIdAsync(userId); // ще го направим след малко

            if (alreadyReferee != null)
            {
                // директно го assign-ваме към турнира
                var success = await refereeService.AssignExistingRefereeToTournamentAsync(userId, id);

                if (!success)
                {
                    return BadRequest();
                }

                return RedirectToAction(nameof(AllTournamentsToReferee));
            }

            // старата логика – ако още не е рефер, показваме формата
            var model = new RefereeFormViewMOdel
            {
                TournamentId = id
            };

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

            // проста валидация
            if (rating < 1 || rating > 5)
            {
                TempData["ErrorMessage"] = "Rating must be between 1 and 5.";
                return RedirectToAction(nameof(Referees));
            }

            await refereeService.RateRefereeAsync(refereeId, userId, rating);

            return RedirectToAction(nameof(Referees));
        }

        [HttpGet]
        public async Task<IActionResult> Invite()
        {
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
            if (string.IsNullOrWhiteSpace(model.RefereeId))
            {
                ModelState.AddModelError(nameof(model.RefereeId), "Моля, изберете рефер.");
            }

            if (string.IsNullOrWhiteSpace(model.Subject))
            {
                ModelState.AddModelError(nameof(model.Subject), "Моля, въведете тема.");
            }

            if (string.IsNullOrWhiteSpace(model.Content))
            {
                ModelState.AddModelError(nameof(model.Content), "Моля, въведете съобщение.");
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

            // тук: admin (User.Id()) изпраща съобщение към рефера (model.RefereeId)
            await contactMessageService.SendInitialAsync(
                User.Id(),            // изпращач (админ)
                model.Subject,
                model.Content,
                model.RefereeId       // получател (рефер)
            );

            TempData["Success"] = "Поканата беше изпратена успешно!";
            return RedirectToAction(nameof(Referees));
        }



    }
}

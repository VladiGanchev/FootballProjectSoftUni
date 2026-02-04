using FootballProjectSoftUni.Core.Contracts.Tournament;
using FootballProjectSoftUni.Core.Extensions;
using FootballProjectSoftUni.Core.Models.Match;
using FootballProjectSoftUni.Extensions;
using FootballProjectSoftUni.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootballProjectSoftUni.Controllers
{
    [Authorize]
    public class MatchController : Controller
    {
        private readonly ITournamentService tournamentService;

        public MatchController(ITournamentService _tournamentService)
        {
            tournamentService = _tournamentService;
        }

        [HttpGet]
        public async Task<IActionResult> EnterResult(int id)
        {
            if (!User.IsAdmin()) return Unauthorized();

            var model = await tournamentService.GetEnterResultModelAsync(id);
            if (model == null) return NotFound();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EnterResult(EnterResultViewModel model)
        {
            if (!User.IsAdmin())
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                var refill0 = await tournamentService.GetEnterResultModelAsync(model.MatchId);
                return View(refill0 ?? model);
            }

            var result = await tournamentService.EnterMatchResultAsync(model);

            if (!result.ok)
            {
                ModelState.AddModelError("", result.error!);

                var refill = await tournamentService.GetEnterResultModelAsync(model.MatchId);
                return View(refill ?? model);
            }

            var details = await tournamentService.GetTournamentDetailsAsync(result.tournamentId);
            var info = details.GetInformation();

            return RedirectToAction("Details", "Tournament", new { id = result.tournamentId, information = info });
        }


    }
}

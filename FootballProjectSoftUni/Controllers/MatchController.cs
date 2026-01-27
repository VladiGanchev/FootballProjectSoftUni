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
        private readonly ApplicationDbContext data;
        private readonly ITournamentService tournamentService;

        public MatchController(ApplicationDbContext _data, ITournamentService _tournamentService)
        {
            data = _data;
            tournamentService = _tournamentService;
        }

        [HttpGet]
        public async Task<IActionResult> EnterResult(int id)
        {
            if (!User.IsAdmin())
            {
                return Unauthorized();
            }

            var match = await data.Matches
                .Include(m => m.Team1)
                .Include(m => m.Team2)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (match == null)
            {
                return NotFound();
            }

            var model = new EnterResultViewModel
            {
                MatchId = match.Id,
                TournamentId = match.TournamentId,
                Team1Id = match.Team1Id,
                Team1Name = match.Team1?.Name,
                Team2Id = match.Team2Id,
                Team2Name = match.Team2?.Name,
                Team1Goals = match.Team1Goals,
                Team2Goals = match.Team2Goals,
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EnterResult(EnterResultViewModel model)
        {
            if (!User.IsAdmin())
            {
                return Unauthorized();
            }

            var match = await data.Matches.FirstOrDefaultAsync(m => m.Id == model.MatchId);

            if (match == null)
            {
                return NotFound();
            }

            if (model.Team1Goals == model.Team2Goals)
            {
                ModelState.AddModelError("", "Равенство не е позволено. Моля, въведи победител.");
            }

            if (model.Team1Goals < 0 || model.Team2Goals < 0)
            {
                ModelState.AddModelError("", "Головете не могат да са отрицателни.");
            }

            if (match.Team1Id.HasValue)
            {
                var t1 = await data.Teams.FindAsync(match.Team1Id.Value);
                model.Team1Name = t1?.Name;
            }

            if (match.Team2Id.HasValue)
            {
                var t2 = await data.Teams.FindAsync(match.Team2Id.Value);
                model.Team2Name = t2?.Name;
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            match.Team1Goals = model.Team1Goals;
            match.Team2Goals = model.Team2Goals;

            if (model.Team1Goals > model.Team2Goals)
            {
                match.WinnerTeamId = match.Team1Id;
            }
            else
            {
                match.WinnerTeamId = match.Team2Id;
            }

            await tournamentService.MoveWinnerToNextRoundAsync(match.Id);
            await data.SaveChangesAsync();

            var details = await tournamentService.GetTournamentDetailsAsync(model.TournamentId);
            var info = details.GetInformation();

            return RedirectToAction("Details", "Tournament", new { id = model.TournamentId, information = info });
        }


    }
}

using FootballProjectSoftUni.Core.Models.Profile;
using FootballProjectSoftUni.Infrastructure.Data;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FootballProjectSoftUni.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;

        public ProfileController(ApplicationDbContext _context, UserManager<ApplicationUser> _userManager)
        {
            context = _context;
            userManager = _userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await userManager.FindByIdAsync(userId);

            // Проверка за роля
            var isReferee = await context.Referees.AnyAsync(r => r.Id == userId);
            var isCoach = await context.Coaches.AnyAsync(c => c.Id == userId);

            var model = new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = isReferee ? "Referee" : isCoach ? "Coach" : null
            };

            // Ако е Coach → зареждаме опит и име на отбор
            if (isCoach)
            {
                var coach = await context.Coaches
                    .Include(c => c.Team)
                    .FirstOrDefaultAsync(c => c.Id == userId);

                if (coach != null)
                {
                    model.Experience = int.Parse(coach.Experience);
                    model.TeamName = coach.Team?.Name;
                }
            }

            // Ако е Referee → брой турнири + средна оценка
            if (isReferee)
            {
                var referee = await context.Referees
                    .Include(r => r.Ratings)
                    .FirstOrDefaultAsync(r => r.Id == userId);

                model.Experience = referee?.Experience;
                model.RefereeTournamentsCount = referee?.RefereedTournamentsCount ?? 0;

                if (referee?.Ratings != null && referee.Ratings.Any())
                {
                    model.RefereeRating = referee.Ratings.Average(r => r.Value);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRefereeRole()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Намираме рефера
            var referee = await context.Referees
                .FirstOrDefaultAsync(r => r.Id == userId);

            if (referee != null)
            {
                // махаме участията му като Referee от TournamentParticipants
                var refereeParticipations = await context.TournamentsParticipants
                    .Where(tp => tp.ParticipantId == userId && tp.Role == "Referee")
                    .ToListAsync();

                context.TournamentsParticipants.RemoveRange(refereeParticipations);

                // ако в някой турнир е закачен като referee → откачаме
                var tournament = await context.Tournaments
                    .FirstOrDefaultAsync(t => t.RefereeId == userId);

                if (tournament != null)
                {
                    tournament.RefereeId = null;
                }

                // трим самия Referee запис
                context.Referees.Remove(referee);

                await context.SaveChangesAsync();
            }

            // след това се връщаме към профила
            return RedirectToAction(nameof(Index));
        }

    }
}

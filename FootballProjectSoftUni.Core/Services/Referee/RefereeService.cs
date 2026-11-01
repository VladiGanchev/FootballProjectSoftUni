using FootballProjectSoftUni.Core.Contracts.Referee;
using FootballProjectSoftUni.Core.Extensions;
using FootballProjectSoftUni.Core.Models.Referee;
using FootballProjectSoftUni.Core.Models.ServiceError;
using FootballProjectSoftUni.Core.Models.Tournament;
using FootballProjectSoftUni.Infrastructure.Data;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Services.Referee
{
    public class RefereeService : IRefereeService
    {
        private readonly ApplicationDbContext context;

        public RefereeService(ApplicationDbContext _context)
        {
            context = _context;
        }

        public async Task<ServiceError> CheckForErrorsAsync(int tournamentId, string userId)
        {
            var tp = await context.TournamentsParticipants
                .Where(x => x.ParticipantId == userId
                            && x.TournamentId == tournamentId
                            && x.Role == "Coach")
                .FirstOrDefaultAsync();

            if (tp != null)
            {
                return new ServiceError()
                {
                    Message = "You cannot apply as a referee for this tournament because you already participate as a coach in it."
                };
            }

            var coach = await context.Coaches
                .FirstOrDefaultAsync(c => c.Id == userId);

            if (coach != null)
            {
                return new ServiceError()
                {
                    Message = "You cannot become a referee because you are already registered as a coach in the system."
                };
            }

            // Busy only while the tournament he referees has not ended; after that he can take another one
            // (the finished tournament keeps him in its history).
            var now = BgTime.Now;
            var activeRefereeInAnotherTournament = await context.Referees
                .AnyAsync(r =>
                    r.Id == userId &&
                    r.IsActive &&
                    r.TournamentId != null &&
                    r.TournamentId != tournamentId &&
                    r.Tournament!.EndDate > now);

            if (activeRefereeInAnotherTournament)
            {
                return new ServiceError()
                {
                    Message = "You cannot apply as a referee for this tournament because you already participate in another one as a referee. You have to leave the current tournament you are in to become a referee to another one."
                };
            }

            var tournament = await context.Tournaments
                .Where(x => x.Id == tournamentId)
                .FirstOrDefaultAsync();

            if (tournament == null)
            {
                return new ServiceError()
                {
                    Message = "Tournament not found."
                };
            }

            if (tournament.RefereeId != null)
            {
                return new ServiceError()
                {
                    Message = "You cannot apply as a referee for this tournament because there is already registered referee."
                };
            }

            return null;
        }


        public async Task<IEnumerable<TournamentViewModel>> GetTournamentsAsync(string userId)
        {
            var tournaments = await context.TournamentsParticipants
                .Where(x => x.ParticipantId == userId && x.Role == "Referee")
                .Select(x => new TournamentViewModel()
                {
                    Id = x.TournamentId,
                    StartDate = x.Tournament.StartDate,
                    CityName = x.Tournament.TournamentCities.FirstOrDefault().City.Name,
                    EndDate = x.Tournament.EndDate,
                    Description = x.Tournament.Description,
                    Status = x.Tournament.Status,
                    NumberOfTeams = x.Tournament.NumberOfTeams,
                    ImageUrl = x.Tournament.ImageUrl,
                })
                .ToListAsync();

            foreach (var t in tournaments)
            {
                t.Information = t.GetInformation();
            }

            return tournaments;
        }

        public async Task<bool> CreateRefereeToTournamentAsync(
     RefereeFormViewMOdel model,
     int id,
     string userId,
     DateTime birthdate)
        {
            var targetTournament = await context.Tournaments.FirstOrDefaultAsync(t => t.Id == id);
            if (targetTournament == null)
            {
                return false;
            }

            // Never replace a referee that is already assigned to the tournament.
            if (targetTournament.RefereeId != null && targetTournament.RefereeId != userId)
            {
                return false;
            }

            var referee = await context.Referees
                .FirstOrDefaultAsync(r => r.Id == userId);

            if (referee == null)
            {
                referee = new FootballProjectSoftUni.Infrastructure.Data.Models.Referee()
                {
                    Id = userId,
                    Name = model.Name,
                    Birthdate = birthdate,
                    Experience = model.Experience
                };

                context.Referees.Add(referee);
            }
            else
            {
                // Becoming a referee again: the old profile comes back, with all its ratings and comments.
                referee.IsActive = true;
                referee.Name = model.Name;
                referee.Birthdate = birthdate;
                referee.Experience = model.Experience;
            }

            referee.TournamentId = id;

            await context.SaveChangesAsync();

            var tournament = await context.Tournaments
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();

            if (tournament == null)
            {
                return false;
            }

            tournament.Referee = referee;
            tournament.RefereeId = referee.Id;

            await context.SaveChangesAsync();

            var alreadyParticipant = await context.TournamentsParticipants
                .AnyAsync(x => x.ParticipantId == userId && x.TournamentId == id && x.Role == "Referee");

            if (!alreadyParticipant)
            {
                TournamentParticipant tp = new TournamentParticipant()
                {
                    ParticipantId = userId,
                    TournamentId = id,
                    Role = "Referee"
                };

                context.TournamentsParticipants.Add(tp);
                await context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> LeaveTournamentAsync(int id, string userId)
        {
            var tournamentId = id;

            var tp = await context.TournamentsParticipants.Where(x => x.ParticipantId == userId && x.TournamentId == tournamentId && x.Role == "Referee").FirstOrDefaultAsync();

            if (tp == null)
            {
                return false;
            }

            var tournament = await context.Tournaments
                .Include(t => t.TournamentCities)
                    .ThenInclude(tc => tc.City)
                .FirstOrDefaultAsync(t => t.Id == tournamentId);

            if (tournament == null)
            {
                return false;
            }

            // Like coaches: only before the start. Nobody can sign up as referee after it, so the
            // tournament would stay without one.
            if (BgTime.Now >= tournament.StartDate)
            {
                return false;
            }

            context.TournamentsParticipants.Remove(tp);

            var referee = await context.Referees.FirstOrDefaultAsync(r => r.Id == userId && r.TournamentId == tournamentId);

            if (referee != null)
            {
                referee.TournamentId = null;
            }

            if (tournament.RefereeId == userId)
            {
                tournament.RefereeId = null;
            }

            await context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<RefereeListItemViewModel>> GetAllRefereesWithRatingsAsync(string userId)
        {
            var referees = await context.Referees
                .Where(r => r.IsActive)
                .Include(r => r.Ratings)
                .ToListAsync();

            var finishedTournaments = await FinishedTournamentsCountAsync();

            var result = referees
                .Select(r =>
                {
                    double? avg = null;
                    if (r.Ratings != null && r.Ratings.Any())
                    {
                        avg = r.Ratings.Average(rr => rr.Value);
                    }

                    return new RefereeListItemViewModel
                    {
                        Id = r.Id,
                        Name = r.Name,
                        Experience = r.Experience,
                        TournamentsCount = finishedTournaments.TryGetValue(r.Id, out var count) ? count : 0,
                        AverageRating = avg
                    };
                })
                .ToList();

            return result;
        }

        public async Task RateRefereeAsync(string refereeId, string userId, int rating)
        {
            if (rating < 1 || rating > 5)
            {
                throw new ArgumentException("Rating must be between 1 and 5.");
            }

            if (refereeId == userId)
            {
                throw new InvalidOperationException("You cannot rate yourself.");
            }

            var referee = await context.Referees
                .FirstOrDefaultAsync(r => r.Id == refereeId && r.IsActive);

            if (referee == null)
            {
                throw new ArgumentException("Invalid referee.");
            }

            var existing = await context.RefereesRatings
                .FirstOrDefaultAsync(r => r.RefereeId == refereeId && r.UserId == userId);

            if (existing == null)
            {
                var newRating = new RefereeRating
                {
                    RefereeId = refereeId,
                    UserId = userId,
                    Value = rating
                };

                context.RefereesRatings.Add(newRating);
            }
            else
            {
                existing.Value = rating;
            }

            await context.SaveChangesAsync();
        }

        public async Task<bool> AssignExistingRefereeToTournamentAsync(string userId, int tournamentId)
        {
            var referee = await context.Referees
                .FirstOrDefaultAsync(r => r.Id == userId && r.IsActive);

            if (referee == null)
            {
                return false;
            }

            var tournament = await context.Tournaments
                .FirstOrDefaultAsync(t => t.Id == tournamentId);

            if (tournament == null)
            {
                return false;
            }

            if (tournament.RefereeId != null)
            {
                return false;
            }

            referee.TournamentId = tournamentId;
            tournament.RefereeId = referee.Id;
            tournament.Referee = referee;

            var existingTp = await context.TournamentsParticipants
                .FirstOrDefaultAsync(tp => tp.ParticipantId == userId
                                           && tp.TournamentId == tournamentId
                                           && tp.Role == "Referee");

            if (existingTp == null)
            {
                var tp = new TournamentParticipant
                {
                    ParticipantId = userId,
                    TournamentId = tournamentId,
                    Role = "Referee"
                };

                context.TournamentsParticipants.Add(tp);
            }

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<Infrastructure.Data.Models.Referee?> GetRefereeByUserIdAsync(string userId)
        {
            // A switched-off profile ("stopped being a referee") does not count as a referee.
            return await context.Referees
                .FirstOrDefaultAsync(r => r.Id == userId && r.IsActive);
        }

        /// <summary>Finished tournaments per referee: what he really refereed (joining and leaving do not count).</summary>
        private async Task<Dictionary<string, int>> FinishedTournamentsCountAsync()
        {
            var now = BgTime.Now;

            var counts = await context.Tournaments
                .Where(t => t.RefereeId != null && t.EndDate < now)
                .GroupBy(t => t.RefereeId!)
                .Select(g => new { RefereeId = g.Key, Count = g.Count() })
                .ToListAsync();

            return counts.ToDictionary(c => c.RefereeId, c => c.Count);
        }

        public async Task<RefereeCommentsPageViewModel> GetCommentsAsync(string refereeId)
        {
            var referee = await context.Referees
                .FirstOrDefaultAsync(r => r.Id == refereeId && r.IsActive);

            if (referee == null)
            {
                throw new ArgumentException("Invalid referee");
            }

            var comments = await context.RefereeComments
       .Where(c => c.RefereeId == refereeId)
       .OrderByDescending(c => c.CreatedOn)
       .Select(c => new RefereeCommentViewModel
       {
           Id = c.Id,
           UserId = c.UserId,
           Content = c.Content,
           CreatedOn = c.CreatedOn,
           UserName = context.Users
               .Where(u => u.Id == c.UserId)
               .Select(u => $"{u.FirstName} {u.LastName}")
               .FirstOrDefault()!
       })
       .ToListAsync();
            return new RefereeCommentsPageViewModel
            {
                RefereeId = refereeId,
                RefereeName = referee.Name,
                Comments = comments
            };
        }

        public async Task<bool> CanCommentAsync(string userId)
        {
            var since = DateTime.UtcNow.AddSeconds(-30);
            return !await context.RefereeComments.AnyAsync(c => c.UserId == userId && c.CreatedOn >= since);
        }

        public async Task AddCommentAsync(string refereeId, string userId, string content)
        {
            var comment = new RefereeComment
            {
                RefereeId = refereeId,
                UserId = userId,
                Content = content
            };

            context.RefereeComments.Add(comment);
            await context.SaveChangesAsync();
        }

        public async Task<RefereeCommentViewModel?> GetCommentAsync(int commentId)
        {
            return await context.RefereeComments
                .Where(c => c.Id == commentId)
                .Select(c => new RefereeCommentViewModel
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    Content = c.Content,
                    CreatedOn = c.CreatedOn,
                    RefereeId = c.RefereeId,
                    RefereeName = c.Referee.Name,
                    UserName = context.Users
                        .Where(u => u.Id == c.UserId)
                        .Select(u => u.FirstName + " " + u.LastName)
                        .FirstOrDefault() ?? string.Empty
                })
                .FirstOrDefaultAsync();
        }

        public async Task<RefereeCommentViewModel?> DeleteCommentAsync(int commentId)
        {
            var deleted = await GetCommentAsync(commentId);
            var comment = await context.RefereeComments.FindAsync(commentId);

            if (deleted == null || comment == null)
            {
                return null;
            }

            context.RefereeComments.Remove(comment);
            await context.SaveChangesAsync();

            return deleted;
        }

        public async Task<string> GetRefereeEmail(string userId)
        {
            var email = await context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.Email)
                .FirstOrDefaultAsync();

            if (email == null)
            {
                throw new ArgumentException("Invalid referee user id.");
            }

            return email;
        }
    }
}

using FootballProjectSoftUni.Core.Contracts.Referee;
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

            var activeRefereeInAnotherTournament = await context.Referees
                .AnyAsync(r =>
                    r.Id == userId &&
                    r.TournamentId != null &&       
                    r.TournamentId != tournamentId  
                );

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
                  Status = x.Tournament.Status.ToString(),
                  NumberOfTeams = x.Tournament.NumberOfTeams,
                  ImageUrl = x.Tournament.ImageUrl,
              })
              .ToListAsync();

            return tournaments;
        }

        public async Task<bool> CreateRefereeToTournamentAsync(
     RefereeFormViewMOdel model,
     int id,
     string userId,
     DateTime birthdate)
        {
            var referee = await context.Referees
                .FirstOrDefaultAsync(r => r.Id == userId);

            if (referee == null)
            {
                referee = new FootballProjectSoftUni.Infrastructure.Data.Models.Referee()
                {
                    Id = userId,
                    Name = model.Name,
                    Birthdate = birthdate,
                    Experience = model.Experience,
                    RefereedTournamentsCount = 0 
                };

                context.Referees.Add(referee);
            }

            referee.RefereedTournamentsCount++;

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

            TournamentParticipant tp = new TournamentParticipant()
            {
                ParticipantId = userId,
                TournamentId = id,
                Role = "Referee"
            };

            context.TournamentsParticipants.Add(tp);
            await context.SaveChangesAsync();

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

            var cityId = tournament.TournamentCities.FirstOrDefault()?.CityId;


            context.TournamentsParticipants.Remove(tp);

            var referee = await context.Referees.FirstOrDefaultAsync(r => r.TournamentId == tournamentId);

            if (referee != null)
            {
                referee.TournamentId = null;
            }

            tournament.RefereeId = null;

            await context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<RefereeListItemViewModel>> GetAllRefereesWithRatingsAsync(string userId)
        {
            var referees = await context.Referees
                .Include(r => r.Ratings)
                .ToListAsync();

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
                        TournamentsCount = r.RefereedTournamentsCount,
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

            var referee = await context.Referees
                .FirstOrDefaultAsync(r => r.Id == refereeId);

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
                .FirstOrDefaultAsync(r => r.Id == userId);

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

            referee.RefereedTournamentsCount++;

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
            return await context.Referees
                .FirstOrDefaultAsync(r => r.Id == userId);
        }

        public async Task<RefereeCommentsPageViewModel> GetCommentsAsync(string refereeId)
        {
            var referee = await context.Referees
                .FirstOrDefaultAsync(r => r.Id == refereeId);

            if (referee == null)
            {
                throw new ArgumentException("Invalid referee");
            }

            var comments = await context.RefereeComments
       .Where(c => c.RefereeId == refereeId)
       .OrderByDescending(c => c.CreatedOn)
       .Select(c => new RefereeCommentViewModel
       {
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

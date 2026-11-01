using FootballProjectSoftUni.Core.Contracts.City;
using FootballProjectSoftUni.Core.Contracts.Coach;
using FootballProjectSoftUni.Core.Contracts.Payment;
using FootballProjectSoftUni.Core.Contracts.Tournament;
using FootballProjectSoftUni.Core.Extensions;
using FootballProjectSoftUni.Core.Models.Tournament;
using FootballProjectSoftUni.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FootballProjectSoftUni.Core.Services.Tournament
{
    public class TournamentDeletionService : ITournamentDeletionService
    {
        // Orders whose money is still with the organiser.
        private static readonly string[] PaidStatuses = { "Paid", "RefundFailed" };

        private readonly ApplicationDbContext context;
        private readonly IPaymentService paymentService;
        private readonly ICoachService coachService;
        private readonly ITournamentService tournamentService;
        private readonly ICityService cityService;

        public TournamentDeletionService(
            ApplicationDbContext context,
            IPaymentService paymentService,
            ICoachService coachService,
            ITournamentService tournamentService,
            ICityService cityService)
        {
            this.context = context;
            this.paymentService = paymentService;
            this.coachService = coachService;
            this.tournamentService = tournamentService;
            this.cityService = cityService;
        }

        public async Task<TournamentDeletionPreview?> PreviewTournamentAsync(int tournamentId)
        {
            var tournament = await context.Tournaments
                .AsNoTracking()
                .Where(t => t.Id == tournamentId)
                .Select(t => new
                {
                    t.Id,
                    t.Description,
                    t.StartDate,
                    t.EndDate,
                    City = t.TournamentCities
                        .Select(tc => new { tc.CityId, tc.City.Name })
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (tournament == null)
            {
                return null;
            }

            var now = BgTime.Now;

            var preview = new TournamentDeletionPreview
            {
                Id = tournament.Id,
                Title = tournament.Description,
                CityId = tournament.City?.CityId,
                CityName = tournament.City?.Name,
                StartDate = tournament.StartDate,
                EndDate = tournament.EndDate,
                IsRunning = IsRunning(tournament.StartDate, tournament.EndDate, now),
                IsFinished = now >= tournament.EndDate
            };

            if (preview.IsRunning)
            {
                preview.RunningUntil = tournament.EndDate;
            }
            else if (!preview.IsFinished)
            {
                await AddPaidTeamsAsync(preview, new[] { tournament.Id });
            }

            return preview;
        }

        public async Task<TournamentDeletionPreview?> PreviewCityAsync(int cityId)
        {
            var city = await context.Cities
                .AsNoTracking()
                .Where(c => c.Id == cityId)
                .Select(c => new { c.Id, c.Name })
                .FirstOrDefaultAsync();

            if (city == null)
            {
                return null;
            }

            var tournaments = await CityTournamentsAsync(cityId);
            var now = BgTime.Now;

            var running = tournaments
                .Where(t => IsRunning(t.StartDate, t.EndDate, now))
                .ToList();

            var preview = new TournamentDeletionPreview
            {
                Id = city.Id,
                Title = city.Name,
                CityId = city.Id,
                CityName = city.Name,
                IsRunning = running.Any(),
                RunningUntil = running.Any() ? running.Max(t => t.EndDate) : null
            };

            var notStartedIds = tournaments
                .Where(t => now < t.StartDate)
                .Select(t => t.Id)
                .ToArray();

            await AddPaidTeamsAsync(preview, notStartedIds);

            return preview;
        }

        public async Task<TournamentDeletionResult> DeleteTournamentAsync(int tournamentId)
        {
            var result = new TournamentDeletionResult();

            var preview = await PreviewTournamentAsync(tournamentId);
            if (preview == null)
            {
                result.Status = TournamentDeletionStatus.NotFound;
                return result;
            }

            result.CityId = preview.CityId;

            if (preview.IsRunning)
            {
                result.Status = TournamentDeletionStatus.Running;
                result.RunningUntil = preview.RunningUntil;
                return result;
            }

            // Not started: the teams paid for something that will not happen - return their money first.
            // Finished: the fee was used for the tournament, nothing is returned.
            return await RefundAndDeleteAsync(tournamentId, preview, result, refund: !preview.IsFinished, ignoreTournamentStart: false);
        }

        public async Task<TournamentDeletionResult> CancelForTooFewTeamsAsync(int tournamentId)
        {
            var result = new TournamentDeletionResult();

            var preview = await PreviewTournamentAsync(tournamentId);
            if (preview == null)
            {
                result.Status = TournamentDeletionStatus.NotFound;
                return result;
            }

            result.CityId = preview.CityId;

            // Called right after the start time: registration is closed and nothing was played,
            // so the teams get their money back although the start has passed.
            return await RefundAndDeleteAsync(tournamentId, preview, result, refund: true, ignoreTournamentStart: true);
        }

        private async Task<TournamentDeletionResult> RefundAndDeleteAsync(
            int tournamentId,
            TournamentDeletionPreview preview,
            TournamentDeletionResult result,
            bool refund,
            bool ignoreTournamentStart)
        {
            if (refund)
            {
                await RefundPaidTeamsAsync(tournamentId, preview.CityName, preview.StartDate!.Value, result, ignoreTournamentStart);

                if (result.FailedTeams.Any())
                {
                    result.Status = TournamentDeletionStatus.RefundsFailed;
                    return result;
                }

                await AddOtherParticipantsAsync(tournamentId, preview.CityName, preview.StartDate.Value, result);
            }

            var pictures = await context.Tournaments
                .AsNoTracking()
                .Where(t => t.Id == tournamentId)
                .Select(t => t.ImageUrl)
                .ToListAsync();

            try
            {
                await tournamentService.DeleteTournamentAsync(tournamentId);
            }
            catch (Exception ex)
            {
                // Refunds already made are kept in the result, so the teams are still notified.
                // Forget the unsaved deletions, or the next SaveChanges (e.g. a notification) would retry them.
                context.ChangeTracker.Clear();

                result.Status = TournamentDeletionStatus.DeleteFailed;
                result.Error = ex;
                return result;
            }

            result.DeletedTournamentIds.Add(tournamentId);
            result.DeletedPictureUrls.AddRange(pictures);
            result.Status = TournamentDeletionStatus.Deleted;
            return result;
        }

        public async Task<TournamentDeletionResult> DeleteCityAsync(int cityId)
        {
            var result = new TournamentDeletionResult { CityId = cityId };

            var preview = await PreviewCityAsync(cityId);
            if (preview == null)
            {
                result.Status = TournamentDeletionStatus.NotFound;
                return result;
            }

            if (preview.IsRunning)
            {
                result.Status = TournamentDeletionStatus.Running;
                result.RunningUntil = preview.RunningUntil;
                return result;
            }

            var tournaments = await CityTournamentsAsync(cityId);
            var notStarted = tournaments
                .Where(t => BgTime.Now < t.StartDate)
                .ToList();

            // First every refund; the city is deleted only if all of them succeeded.
            foreach (var tournament in notStarted)
            {
                await RefundPaidTeamsAsync(tournament.Id, preview.CityName, tournament.StartDate, result);
            }

            if (result.FailedTeams.Any())
            {
                result.Status = TournamentDeletionStatus.RefundsFailed;
                return result;
            }

            foreach (var tournament in notStarted)
            {
                await AddOtherParticipantsAsync(tournament.Id, preview.CityName, tournament.StartDate, result);
            }

            var pictures = await context.Cities
                .AsNoTracking()
                .Where(c => c.Id == cityId)
                .Select(c => c.ImageUrl)
                .ToListAsync();

            pictures.AddRange(await context.TournamentsCities
                .AsNoTracking()
                .Where(tc => tc.CityId == cityId)
                .Select(tc => tc.Tournament.ImageUrl)
                .ToListAsync());

            // One transaction: the city, its tournaments and its ranking go together or not at all.
            try
            {
                await cityService.DeleteCityAsync(cityId);
            }
            catch (Exception ex)
            {
                // Forget the unsaved deletions, or the next SaveChanges (e.g. a notification) would retry them.
                context.ChangeTracker.Clear();

                result.Status = TournamentDeletionStatus.DeleteFailed;
                result.Error = ex;
                return result;
            }

            result.DeletedTournamentIds.AddRange(tournaments.Select(t => t.Id));
            result.DeletedPictureUrls.AddRange(pictures);
            result.Status = TournamentDeletionStatus.Deleted;
            return result;
        }

        private static bool IsRunning(DateTime start, DateTime end, DateTime now) =>
            now >= start && now < end;

        private async Task<List<(int Id, DateTime StartDate, DateTime EndDate)>> CityTournamentsAsync(int cityId)
        {
            var tournaments = await context.TournamentsCities
                .AsNoTracking()
                .Where(tc => tc.CityId == cityId)
                .Select(tc => new { tc.Tournament.Id, tc.Tournament.StartDate, tc.Tournament.EndDate })
                .ToListAsync();

            return tournaments
                .Select(t => (t.Id, t.StartDate, t.EndDate))
                .ToList();
        }

        private async Task AddPaidTeamsAsync(TournamentDeletionPreview preview, int[] tournamentIds)
        {
            if (tournamentIds.Length == 0)
            {
                return;
            }

            var amounts = await context.TournamentJoinPayments
                .AsNoTracking()
                .Where(o => o.TournamentId.HasValue
                         && tournamentIds.Contains(o.TournamentId.Value)
                         && PaidStatuses.Contains(o.Status))
                .Select(o => o.Amount)
                .ToListAsync();

            preview.PaidTeams = amounts.Count;
            preview.PaidTotal = amounts.Sum();
        }

        private async Task RefundPaidTeamsAsync(int tournamentId, string? city, DateTime startDate, TournamentDeletionResult result, bool ignoreTournamentStart = false)
        {
            // Nobody can pay for this tournament any more.
            await paymentService.ExpireOpenCheckoutsAsync(tournamentId);

            var orders = await context.TournamentJoinPayments
                .AsNoTracking()
                .Where(o => o.TournamentId == tournamentId && PaidStatuses.Contains(o.Status))
                .Select(o => new
                {
                    o.Id,
                    o.UserId,
                    o.Culture,
                    o.Amount,
                    TeamName = context.Teams
                        .Where(t => t.Id == o.TeamId)
                        .Select(t => t.Name)
                        .FirstOrDefault()
                })
                .ToListAsync();

            foreach (var order in orders)
            {
                bool refunded;
                try
                {
                    refunded = await paymentService.RefundTournamentJoinAsync(order.Id, null, "requested_by_customer", ignoreTournamentStart);
                }
                catch (Exception)
                {
                    refunded = false;
                }

                if (!refunded)
                {
                    result.FailedTeams.Add(order.TeamName ?? $"#{order.Id}");
                    continue;
                }

                // The money is back, so the team is out - even if another refund fails and the tournament stays.
                await coachService.LeaveTournamentAsync(tournamentId, order.UserId);

                result.Refunded.Add(new RefundedTeam
                {
                    UserId = order.UserId,
                    Culture = order.Culture,
                    Amount = order.Amount,
                    City = city,
                    StartDate = startDate
                });
            }
        }

        private async Task AddOtherParticipantsAsync(int tournamentId, string? city, DateTime startDate, TournamentDeletionResult result)
        {
            var participantIds = await context.TournamentsParticipants
                .AsNoTracking()
                .Where(tp => tp.TournamentId == tournamentId)
                .Select(tp => tp.ParticipantId)
                .Distinct()
                .ToListAsync();

            foreach (var userId in participantIds.Where(id => result.Refunded.All(r => r.UserId != id)))
            {
                result.OtherParticipants.Add(new CanceledParticipant
                {
                    UserId = userId,
                    City = city,
                    StartDate = startDate
                });
            }
        }
    }
}

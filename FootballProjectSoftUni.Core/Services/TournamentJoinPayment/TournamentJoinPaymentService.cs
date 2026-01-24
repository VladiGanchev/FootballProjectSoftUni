using FootballProjectSoftUni.Core.Contracts.TournamentJoinPayment;
using FootballProjectSoftUni.Core.Models.Settings;
using FootballProjectSoftUni.Core.Models.TournamentJoinPayment;
using FootballProjectSoftUni.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Services.TournamentJoinPayment
{
    public class TournamentJoinPaymentService : ITournamentJoinPaymentService
    {
        private readonly ApplicationDbContext context;

        public TournamentJoinPaymentService(
            ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<TournamentJoinPaymentViewModel?> GetTournametJoinPaymentOrder(int orderId)
        {
            return await context.TournamentJoinPayments
                .Where(o => o.Id == orderId)
                .Select(o => new TournamentJoinPaymentViewModel
                {
                    Id = o.Id,
                    Status = o.Status,
                    TournamentId = o.TournamentId,
                    TeamId = o.TeamId,
                    Amount = o.Amount,
                    Currency = o.Currency,
                    CreatedOnUtc = o.CreatedOnUtc,
                    PaidOnUtc = o.PaidOnUtc,
                    StripePaymentIntentId = o.StripePaymentIntentId
                })
                .FirstOrDefaultAsync();
        }
    }
}

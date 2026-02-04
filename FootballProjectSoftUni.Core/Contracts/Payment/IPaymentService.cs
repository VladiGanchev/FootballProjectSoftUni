using FootballProjectSoftUni.Core.Models.AdminPayments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Contracts.Payment
{
    public interface IPaymentService
    {
        Task<string> CreateTournamentJoinCheckoutAsync(int tournamentId, string userId, int? teamId);
        Task<bool> RefundTournamentJoinAsync(int orderId, decimal? amount = null, string? reason = null);
        Task<ICollection<AdminPaymentViewModel>> GetAllTournamentJoinPaymentsAsync();

    }
}

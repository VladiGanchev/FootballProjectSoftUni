using FootballProjectSoftUni.Core.Models.TournamentJoinPayment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Contracts.TournamentJoinPayment
{
    public interface ITournamentJoinPaymentService
    {
        Task<TournamentJoinPaymentViewModel> GetTournametJoinPaymentOrder(int orderId);
    }
}

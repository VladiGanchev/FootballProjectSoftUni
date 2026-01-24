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

    }
}

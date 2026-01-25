using FootballProjectSoftUni.Core.Contracts.Payment;
using FootballProjectSoftUni.Core.Models.AdminPayments;
using FootballProjectSoftUni.Extensions;
using FootballProjectSoftUni.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FootballProjectSoftUni.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IPaymentService paymentService;

        public PaymentsController(ApplicationDbContext context, IPaymentService paymentService)
        {
            this.context = context;
            this.paymentService = paymentService;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsAdmin() == false)
            {
                return Unauthorized();
            }

            var payments = await context.TournamentJoinPayments
                .OrderByDescending(p => p.PaidOnUtc)
                .Select(p => new AdminPaymentViewModel
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    TournamentId = p.TournamentId,
                    TeamId = p.TeamId,
                    Amount = p.Amount,
                    Status = p.Status,
                    PaidOnUtc = p.PaidOnUtc
                })
                .ToListAsync();

            return View(payments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Refund(int orderId)
        {
            try
            {
                await paymentService.RefundTournamentJoinAsync(orderId, amount: null, reason: "requested_by_customer");
                TempData["Success"] = "Refund-ът е пуснат успешно.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Index"); 
        }
    }
}

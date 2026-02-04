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
        private readonly IPaymentService paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            this.paymentService = paymentService;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsAdmin() == false)
            {
                return Unauthorized();
            }

            var payments = await paymentService.GetAllTournamentJoinPaymentsAsync();
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

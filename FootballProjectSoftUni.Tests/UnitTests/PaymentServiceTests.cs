using FootballProjectSoftUni.Core.Models.AdminPayments;
using FootballProjectSoftUni.Core.Models.Payment;
﻿using FootballProjectSoftUni.Core.Models.Settings;
using FootballProjectSoftUni.Core.Services.Payment;
using FootballProjectSoftUni.Infrastructure.Data.Enums;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Tests.UnitTests
{
    public class PaymentServiceTests : UnitTestsBase
    {
        [Test]
        public async Task GetAllTournamentJoinPaymentsAsync_ShouldReturnEmpty_WhenNoPayments()
        {
            var service = new PaymentService(_data, Options.Create(new StripeSettings { Currency = "EUR" }), new HttpContextAccessor());

            var result = await service.GetAllTournamentJoinPaymentsAsync();

            Assert.IsNotNull(result);
            Assert.IsEmpty(result);
        }

        [Test]
        public async Task GetAllTournamentJoinPaymentsAsync_ShouldReturnAllPayments_OrderedByPaidOnUtcDesc()
        {
            var service = new PaymentService(_data, Options.Create(new StripeSettings { Currency = "EUR" }), new HttpContextAccessor());

            await _data.TournamentJoinPayments.AddRangeAsync(
                new FootballProjectSoftUni.Infrastructure.Data.Models.TournamentJoinPayment
                {
                    Id = 1,
                    UserId = "u1",
                    TournamentId = 10,
                    TeamId = 1,
                    Amount = 10m,
                    Currency = "EUR",
                    Status = "Paid",
                    PaidOnUtc = new DateTime(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc)
                },
                new FootballProjectSoftUni.Infrastructure.Data.Models.TournamentJoinPayment
                {
                    Id = 2,
                    UserId = "u2",
                    TournamentId = 11,
                    TeamId = 2,
                    Amount = 20m,
                    Currency = "EUR",
                    Status = "Paid",
                    PaidOnUtc = new DateTime(2031, 1, 1, 10, 0, 0, DateTimeKind.Utc)
                }
            );
            await _data.SaveChangesAsync();

            var result = (await service.GetAllTournamentJoinPaymentsAsync()).ToList();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Id, Is.EqualTo(2));
            Assert.That(result[1].Id, Is.EqualTo(1));
        }

        //[Test]
        //public void CreateTournamentJoinCheckoutAsync_ShouldThrow_WhenTournamentNotFound()
        //{
        //    var http = new HttpContextAccessor
        //    {
        //        HttpContext = new DefaultHttpContext()
        //    };
        //    http.HttpContext.Request.Scheme = "https";
        //    http.HttpContext.Request.Host = new HostString("localhost");

        //    var service = new PaymentService(_data, Options.Create(new StripeSettings { Currency = "EUR" }), http);

        //    Assert.ThrowsAsync<ArgumentException>(async () =>
        //        await service.CreateTournamentJoinCheckoutAsync(99999, "u1", 1));
        //}

        //[Test]
        //public async Task CreateTournamentJoinCheckoutAsync_ShouldThrow_WhenFeeNotConfigured()
        //{
        //    var http = new HttpContextAccessor
        //    {
        //        HttpContext = new DefaultHttpContext()
        //    };
        //    http.HttpContext.Request.Scheme = "https";
        //    http.HttpContext.Request.Host = new HostString("localhost");

        //    var t = new Tournament
        //    {
        //        Id = 30001,
        //        StartDate = DateTime.Now.AddDays(5),
        //        EndDate = DateTime.Now.AddDays(6),
        //        CreatedOn = DateTime.Now,
        //        Description = "T",
        //        OrganiserId = "org",
        //        NumberOfTeams = 0,
        //        ImageUrl = "img",
        //        Status = TournamentStatus.Upcoming,
        //        Prize = 0,
        //        ParticipationFee = 0m,
        //        ReminderSent = false
        //    };

        //    await _data.Tournaments.AddAsync(t);
        //    await _data.SaveChangesAsync();

        //    var service = new PaymentService(_data, Options.Create(new StripeSettings { Currency = "EUR" }), http);

        //    var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
        //        await service.CreateTournamentJoinCheckoutAsync(30001, "u1", 1));

        //    Assert.That(ex!.Message, Is.EqualTo("Participation fee is not configured."));
        //}

        [Test]
        public void RefundTournamentJoinAsync_ShouldThrow_WhenOrderNotFound()
        {
            var service = new PaymentService(_data, Options.Create(new StripeSettings { Currency = "EUR" }), new HttpContextAccessor());

            Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.RefundTournamentJoinAsync(99999));
        }

        [Test]
        public async Task RefundTournamentJoinAsync_ShouldThrow_WhenOrderNotPaid()
        {
            var service = new PaymentService(_data, Options.Create(new StripeSettings { Currency = "EUR" }), new HttpContextAccessor());

            await _data.TournamentJoinPayments.AddAsync(new FootballProjectSoftUni.Infrastructure.Data.Models.TournamentJoinPayment
            {
                Id = 40001,
                UserId = "u1",
                TournamentId = 1,
                Amount = 10m,
                Currency = "EUR",
                Status = "Pending",
                StripePaymentIntentId = "pi_x"
            });
            await _data.SaveChangesAsync();

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.RefundTournamentJoinAsync(40001));

            Assert.That(ex!.Message, Is.EqualTo("Only PAID orders can be refunded."));
        }

        [Test]
        public async Task RefundTournamentJoinAsync_ShouldThrow_WhenAlreadyRefundedOrRefundStarted()
        {
            var service = new PaymentService(_data, Options.Create(new StripeSettings { Currency = "EUR" }), new HttpContextAccessor());

            await _data.TournamentJoinPayments.AddAsync(new FootballProjectSoftUni.Infrastructure.Data.Models.TournamentJoinPayment
            {
                Id = 40002,
                UserId = "u1",
                TournamentId = 1,
                Amount = 10m,
                Currency = "EUR",
                Status = "Paid",
                StripePaymentIntentId = "pi_x",
                StripeRefundId = "re_123"
            });
            await _data.SaveChangesAsync();

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.RefundTournamentJoinAsync(40002));

            Assert.That(ex!.Message, Is.EqualTo("This order is already refunded (or refund is started)."));
        }

        [Test]
        public async Task RefundTournamentJoinAsync_ShouldThrow_WhenMissingPaymentIntentId()
        {
            var service = new PaymentService(_data, Options.Create(new StripeSettings { Currency = "EUR" }), new HttpContextAccessor());

            await _data.TournamentJoinPayments.AddAsync(new FootballProjectSoftUni.Infrastructure.Data.Models.TournamentJoinPayment
            {
                Id = 40003,
                UserId = "u1",
                TournamentId = 1,
                Amount = 10m,
                Currency = "EUR",
                Status = "Paid",
                StripePaymentIntentId = ""
            });
            await _data.SaveChangesAsync();

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.RefundTournamentJoinAsync(40003));

            Assert.That(ex!.Message, Is.EqualTo("Missing Stripe PaymentIntentId for this order."));
        }

        [Test]
        public async Task RefundTournamentJoinAsync_ShouldThrow_WhenRefundAmountInvalidOrTooLarge()
        {
            var service = new PaymentService(_data, Options.Create(new StripeSettings { Currency = "EUR" }), new HttpContextAccessor());

            await _data.TournamentJoinPayments.AddAsync(new FootballProjectSoftUni.Infrastructure.Data.Models.TournamentJoinPayment
            {
                Id = 40004,
                UserId = "u1",
                TournamentId = 1,
                Amount = 10m,
                Currency = "EUR",
                Status = "Paid",
                StripePaymentIntentId = "pi_x"
            });
            await _data.SaveChangesAsync();

            Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.RefundTournamentJoinAsync(40004, amount: 0m));

            Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.RefundTournamentJoinAsync(40004, amount: 999m));
        }


        [Test]
        public async Task RefundTournamentJoinAsync_ShouldThrow_WhenTournamentAlreadyStarted()
        {
            var service = new PaymentService(_data, Options.Create(new StripeSettings { Currency = "EUR" }), new HttpContextAccessor());

            await _data.Tournaments.AddAsync(new Tournament
            {
                Id = 50001,
                StartDate = DateTime.Now.AddHours(-1),
                EndDate = DateTime.Now.AddDays(1),
                CreatedOn = DateTime.Now,
                Description = "started tournament",
                Status = TournamentStatus.Started,
                OrganiserId = "org"
            });
            await _data.TournamentJoinPayments.AddAsync(new FootballProjectSoftUni.Infrastructure.Data.Models.TournamentJoinPayment
            {
                Id = 40010,
                UserId = "u1",
                TournamentId = 50001,
                Amount = 10m,
                Currency = "EUR",
                Status = "Paid",
                StripePaymentIntentId = "pi_x"
            });
            await _data.SaveChangesAsync();

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.RefundTournamentJoinAsync(40010));

            Assert.That(ex!.Message, Is.EqualTo("Refunds are possible only before the tournament starts."));

            var order = await _data.TournamentJoinPayments.FindAsync(40010);
            Assert.That(order!.Status, Is.EqualTo("Paid"));
        }

        // ---------- Confirming a Stripe checkout (webhook) ----------

        private PaymentService CheckoutService() =>
            new PaymentService(_data, Options.Create(new StripeSettings { Currency = "eur" }), new HttpContextAccessor());

        private async Task<FootballProjectSoftUni.Infrastructure.Data.Models.TournamentJoinPayment> AddOrderAsync(int id, string status, string sessionId = "cs_test_1")
        {
            var order = new FootballProjectSoftUni.Infrastructure.Data.Models.TournamentJoinPayment
            {
                Id = id,
                UserId = "u1",
                TournamentId = 60001,
                TeamId = 5,
                Amount = 25.50m,
                Currency = "eur",
                Status = status,
                StripeSessionId = sessionId
            };
            await _data.TournamentJoinPayments.AddAsync(order);
            await _data.SaveChangesAsync();
            return order;
        }

        private static CompletedCheckout PaidCheckout(string sessionId = "cs_test_1", string? orderId = null) => new CompletedCheckout
        {
            SessionId = sessionId,
            OrderId = orderId,
            PaymentStatus = "paid",
            AmountTotal = 2550,
            Currency = "eur",
            PaymentIntentId = "pi_test_1"
        };

        private async Task<string> StatusOfAsync(int id) =>
            (await _data.TournamentJoinPayments.AsNoTracking().FirstAsync(o => o.Id == id)).Status;

        [Test]
        public async Task ConfirmCheckout_PendingOrder_BecomesPaidAndJoins()
        {
            await AddOrderAsync(70001, "Pending");

            var result = await CheckoutService().ConfirmCheckoutPaymentAsync(PaidCheckout(orderId: "70001"));

            Assert.That(result.Outcome, Is.EqualTo(CheckoutPaymentOutcome.Join));
            var order = await _data.TournamentJoinPayments.AsNoTracking().FirstAsync(o => o.Id == 70001);
            Assert.That(order.Status, Is.EqualTo("Paid"));
            Assert.That(order.StripePaymentIntentId, Is.EqualTo("pi_test_1"));
            Assert.IsNotNull(order.PaidOnUtc);
        }

        [Test]
        public async Task ConfirmCheckout_SessionFromAnotherSite_IsIgnoredEvenIfOrderIdMatches()
        {
            // Same order number on this site, but the payment belongs to another copy of the site (other session).
            await AddOrderAsync(70002, "Pending", sessionId: "cs_test_this_site");

            var result = await CheckoutService().ConfirmCheckoutPaymentAsync(PaidCheckout(sessionId: "cs_test_other_site", orderId: "70002"));

            Assert.That(result.Outcome, Is.EqualTo(CheckoutPaymentOutcome.UnknownSession));
            Assert.That(await StatusOfAsync(70002), Is.EqualTo("Pending"));
        }

        [Test]
        public async Task ConfirmCheckout_WrongAmount_IsRejected()
        {
            await AddOrderAsync(70003, "Pending");
            var checkout = PaidCheckout();
            checkout.AmountTotal = 100;

            var result = await CheckoutService().ConfirmCheckoutPaymentAsync(checkout);

            Assert.That(result.Outcome, Is.EqualTo(CheckoutPaymentOutcome.Rejected));
            Assert.That(await StatusOfAsync(70003), Is.EqualTo("Pending"));
        }

        [Test]
        public async Task ConfirmCheckout_WrongCurrency_IsRejected()
        {
            await AddOrderAsync(70004, "Pending");
            var checkout = PaidCheckout();
            checkout.Currency = "usd";

            var result = await CheckoutService().ConfirmCheckoutPaymentAsync(checkout);

            Assert.That(result.Outcome, Is.EqualTo(CheckoutPaymentOutcome.Rejected));
            Assert.That(await StatusOfAsync(70004), Is.EqualTo("Pending"));
        }

        [Test]
        public async Task ConfirmCheckout_NotPaid_IsRejected()
        {
            await AddOrderAsync(70005, "Pending");
            var checkout = PaidCheckout();
            checkout.PaymentStatus = "unpaid";

            var result = await CheckoutService().ConfirmCheckoutPaymentAsync(checkout);

            Assert.That(result.Outcome, Is.EqualTo(CheckoutPaymentOutcome.Rejected));
            Assert.That(await StatusOfAsync(70005), Is.EqualTo("Pending"));
        }

        [Test]
        public async Task ConfirmCheckout_OrderIdNotMatchingSession_IsRejected()
        {
            await AddOrderAsync(70006, "Pending");

            var result = await CheckoutService().ConfirmCheckoutPaymentAsync(PaidCheckout(orderId: "12"));

            Assert.That(result.Outcome, Is.EqualTo(CheckoutPaymentOutcome.Rejected));
            Assert.That(await StatusOfAsync(70006), Is.EqualTo("Pending"));
        }

        [Test]
        public async Task ConfirmCheckout_RefundedOrder_StaysRefunded()
        {
            // Stripe sends the event again after the admin refunded: the team must not come back.
            await AddOrderAsync(70007, "Refunded");

            var result = await CheckoutService().ConfirmCheckoutPaymentAsync(PaidCheckout());

            Assert.That(result.Outcome, Is.EqualTo(CheckoutPaymentOutcome.AlreadyProcessed));
            Assert.That(await StatusOfAsync(70007), Is.EqualTo("Refunded"));
        }

        [Test]
        public async Task ConfirmCheckout_ExpiredOrder_IsPaidAndMustBeRefunded()
        {
            await AddOrderAsync(70008, "Expired");

            var result = await CheckoutService().ConfirmCheckoutPaymentAsync(PaidCheckout());

            Assert.That(result.Outcome, Is.EqualTo(CheckoutPaymentOutcome.Refund));
            Assert.That(await StatusOfAsync(70008), Is.EqualTo("Paid"));
        }

        [Test]
        public async Task ConfirmCheckout_SameEventTwice_JoinsOnlyOnce()
        {
            await AddOrderAsync(70009, "Pending");
            var service = CheckoutService();

            var first = await service.ConfirmCheckoutPaymentAsync(PaidCheckout());
            var second = await service.ConfirmCheckoutPaymentAsync(PaidCheckout());

            Assert.That(first.Outcome, Is.EqualTo(CheckoutPaymentOutcome.Join));
            Assert.That(second.Outcome, Is.EqualTo(CheckoutPaymentOutcome.AlreadyProcessed));
        }

        // ---------- leaving and coming back ----------

        private async Task AddLeaveTestTournamentAsync(int id)
        {
            await _data.Tournaments.AddAsync(new Tournament
            {
                Id = id,
                StartDate = DateTime.Now.AddDays(3),
                EndDate = DateTime.Now.AddDays(4),
                Description = "Leave test",
                OrganiserId = "org",
                CreatedOn = DateTime.Now,
                ImageUrl = "https://example.com/t.jpg",
                Status = TournamentStatus.Upcoming,
                ParticipationFee = 10m
            });
            await _data.SaveChangesAsync();
        }

        private async Task AddOrderAsync(int id, int tournamentId, string status, int teamId = 5)
        {
            await _data.TournamentJoinPayments.AddAsync(new FootballProjectSoftUni.Infrastructure.Data.Models.TournamentJoinPayment
            {
                Id = id,
                UserId = "coach-x",
                TournamentId = tournamentId,
                TeamId = teamId,
                Amount = 10m,
                Currency = "eur",
                Status = status,
                PaidOnUtc = DateTime.UtcNow
            });
            await _data.SaveChangesAsync();
        }

        [Test]
        public async Task ReusePaidOrder_ComingBackAfterLeaving_UsesTheFeeAlreadyPaid()
        {
            await AddLeaveTestTournamentAsync(81001);
            await AddOrderAsync(81101, 81001, "Paid", teamId: 5);
            var service = CheckoutService();

            var orderId = await service.ReusePaidOrderAsync(81001, "coach-x", teamId: 6);

            Assert.That(orderId, Is.EqualTo(81101));
            var order = await _data.TournamentJoinPayments.AsNoTracking().FirstAsync(o => o.Id == 81101);
            Assert.That(order.TeamId, Is.EqualTo(6), "The payment follows the coach's current team.");
            Assert.That(order.Status, Is.EqualTo("Paid"));
        }

        [Test]
        public async Task ReusePaidOrder_AfterTheOrganiserRefunded_PaysAgain()
        {
            await AddLeaveTestTournamentAsync(81002);
            await AddOrderAsync(81102, 81002, "Refunded");

            Assert.IsNull(await CheckoutService().ReusePaidOrderAsync(81002, "coach-x", teamId: 5));
        }

        [Test]
        public async Task CreateCheckout_WhileTheEarlierPaymentIsBeingRefunded_SaysSo()
        {
            await AddLeaveTestTournamentAsync(81003);
            await AddOrderAsync(81103, 81003, "RefundPending");

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await CheckoutService().CreateTournamentJoinCheckoutAsync(81003, "coach-x", 5, true, "declaration"));

            Assert.That(ex!.Message, Is.EqualTo("Your previous payment for this tournament is still being refunded."));
        }

        [Test]
        public async Task CreateCheckout_WhenAlreadyPaid_IsRefused()
        {
            await AddLeaveTestTournamentAsync(81004);
            await AddOrderAsync(81104, 81004, "Paid");

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await CheckoutService().CreateTournamentJoinCheckoutAsync(81004, "coach-x", 5, true, "declaration"));

            Assert.That(ex!.Message, Is.EqualTo("You have already paid for this tournament."));
        }

        [Test]
        public async Task AdminPayments_ShowWhichPaidTeamsLeft()
        {
            await AddLeaveTestTournamentAsync(81005);
            await AddOrderAsync(81105, 81005, "Paid", teamId: 7);   // still in the tournament
            await AddOrderAsync(81106, 81005, "Paid", teamId: 8);   // left
            await _data.TournamentsTeams.AddAsync(new TournamentTeam { TournamentId = 81005, TeamId = 7 });
            await _data.SaveChangesAsync();

            var payments = await CheckoutService().GetAllTournamentJoinPaymentsAsync();

            Assert.IsFalse(payments.Single(p => p.Id == 81105).TeamLeft);
            Assert.IsTrue(payments.Single(p => p.Id == 81106).TeamLeft);
        }

        [Test]
        public void AdminPaymentViewModel_CanRefund_OnlyPaidBeforeStart()
        {
            var future = DateTime.Now.AddDays(2);
            var past = DateTime.Now.AddDays(-2);

            Assert.IsTrue(new AdminPaymentViewModel { Status = "Paid", TournamentStartDate = future }.CanRefund);
            Assert.IsTrue(new AdminPaymentViewModel { Status = "RefundFailed", TournamentStartDate = future }.CanRefund);
            Assert.IsFalse(new AdminPaymentViewModel { Status = "Paid", TournamentStartDate = past }.CanRefund);
            Assert.IsFalse(new AdminPaymentViewModel { Status = "Refunded", TournamentStartDate = future }.CanRefund);
            Assert.IsFalse(new AdminPaymentViewModel { Status = "Pending", TournamentStartDate = future }.CanRefund);
        }
    }
}

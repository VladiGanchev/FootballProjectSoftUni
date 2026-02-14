using FootballProjectSoftUni.Core.Models.Settings;
using FootballProjectSoftUni.Core.Services.Payment;
using FootballProjectSoftUni.Infrastructure.Data.Enums;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Http;
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

        [Test]
        public void CreateTournamentJoinCheckoutAsync_ShouldThrow_WhenTournamentNotFound()
        {
            var http = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };
            http.HttpContext.Request.Scheme = "https";
            http.HttpContext.Request.Host = new HostString("localhost");

            var service = new PaymentService(_data, Options.Create(new StripeSettings { Currency = "EUR" }), http);

            Assert.ThrowsAsync<ArgumentException>(async () =>
                await service.CreateTournamentJoinCheckoutAsync(99999, "u1", 1));
        }

        [Test]
        public async Task CreateTournamentJoinCheckoutAsync_ShouldThrow_WhenFeeNotConfigured()
        {
            var http = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };
            http.HttpContext.Request.Scheme = "https";
            http.HttpContext.Request.Host = new HostString("localhost");

            var t = new Tournament
            {
                Id = 30001,
                StartDate = DateTime.Now.AddDays(5),
                EndDate = DateTime.Now.AddDays(6),
                CreatedOn = DateTime.Now,
                Description = "T",
                OrganiserId = "org",
                NumberOfTeams = 0,
                ImageUrl = "img",
                Status = TournamentStatus.Upcoming,
                Prize = 0,
                ParticipationFee = 0m,
                ReminderSent = false
            };

            await _data.Tournaments.AddAsync(t);
            await _data.SaveChangesAsync();

            var service = new PaymentService(_data, Options.Create(new StripeSettings { Currency = "EUR" }), http);

            var ex = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await service.CreateTournamentJoinCheckoutAsync(30001, "u1", 1));

            Assert.That(ex!.Message, Is.EqualTo("Participation fee is not configured."));
        }

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

    }
}

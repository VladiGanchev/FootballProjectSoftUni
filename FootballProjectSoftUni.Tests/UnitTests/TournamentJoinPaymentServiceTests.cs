using FootballProjectSoftUni.Core.Services.TournamentJoinPayment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FootballProjectSoftUni.Tests.UnitTests
{
    public class TournamentJoinPaymentServiceTests : UnitTestsBase
    {
        [Test]
        public async Task GetTournametJoinPaymentOrder_ShouldReturnNull_WhenOrderNotFound()
        {
            var service = new TournamentJoinPaymentService(_data);

            var result = await service.GetTournametJoinPaymentOrder(99999);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetTournametJoinPaymentOrder_ShouldReturnMappedViewModel_WhenOrderExists()
        {
            var service = new TournamentJoinPaymentService(_data);

            var order = new FootballProjectSoftUni.Infrastructure.Data.Models.TournamentJoinPayment
            {
                Id = 9001,
                UserId = "u1",
                Status = "Paid",
                TournamentId = 100,
                TeamId = 200,
                Amount = 49.99m,
                Currency = "EUR",
                CreatedOnUtc = new DateTime(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                PaidOnUtc = new DateTime(2030, 1, 1, 10, 5, 0, DateTimeKind.Utc),
                StripePaymentIntentId = "pi_test_123"
            };

            await _data.TournamentJoinPayments.AddAsync(order);
            await _data.SaveChangesAsync();

            var result = await service.GetTournametJoinPaymentOrder(9001);

            Assert.NotNull(result);
            Assert.That(result!.Id, Is.EqualTo(9001));
            Assert.That(result.Status, Is.EqualTo("Paid"));
            Assert.That(result.TournamentId, Is.EqualTo(100));
            Assert.That(result.TeamId, Is.EqualTo(200));
            Assert.That(result.Amount, Is.EqualTo(49.99m));
            Assert.That(result.Currency, Is.EqualTo("EUR"));
            Assert.That(result.CreatedOnUtc, Is.EqualTo(new DateTime(2030, 1, 1, 10, 0, 0, DateTimeKind.Utc)));
            Assert.That(result.PaidOnUtc, Is.EqualTo(new DateTime(2030, 1, 1, 10, 5, 0, DateTimeKind.Utc)));
            Assert.That(result.StripePaymentIntentId, Is.EqualTo("pi_test_123"));
        }

        [Test]
        public async Task GetTournametJoinPaymentOrder_ShouldReturnOnlyRequestedOrder_WhenMultipleExist()
        {
            var service = new TournamentJoinPaymentService(_data);

            await _data.TournamentJoinPayments.AddRangeAsync(
                new FootballProjectSoftUni.Infrastructure.Data.Models.TournamentJoinPayment
                {
                    Id = 9101,
                    UserId = "u1",
                    Status = "Pending",
                    TournamentId = 1,
                    TeamId = 1,
                    Amount = 10m,
                    Currency = "EUR",
                    CreatedOnUtc = DateTime.UtcNow,
                    StripePaymentIntentId = "pi_1"
                },
                new FootballProjectSoftUni.Infrastructure.Data.Models.TournamentJoinPayment
                {
                    Id = 9102,
                    UserId = "u2",
                    Status = "Paid",
                    TournamentId = 2,
                    TeamId = 2,
                    Amount = 20m,
                    Currency = "EUR",
                    CreatedOnUtc = DateTime.UtcNow,
                    StripePaymentIntentId = "pi_2"
                }
            );
            await _data.SaveChangesAsync();

            var result = await service.GetTournametJoinPaymentOrder(9102);

            Assert.NotNull(result);
            Assert.That(result!.Id, Is.EqualTo(9102));
            Assert.That(result.Status, Is.EqualTo("Paid"));
            Assert.That(result.StripePaymentIntentId, Is.EqualTo("pi_2"));
        }


    }
}

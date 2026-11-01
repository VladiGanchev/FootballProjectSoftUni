using FootballProjectSoftUni.Core.Contracts.Email;
using FootballProjectSoftUni.Core.Services.EmailSender;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Tests.UnitTests
{
    public class EmailSenderServiceTests
    {
        private class FakeEmailQueue : IEmailQueue
        {
            public List<(string to, string subject, string body)> Sent { get; } = new();

            public void Enqueue(string to, string subject, string htmlBody) => Sent.Add((to, subject, htmlBody));
        }

        [Test]
        public async Task SendEmailAsync_ShouldQueueTheEmail_WithSameParameters()
        {
            var fake = new FakeEmailQueue();
            var sender = new EmailSenderService(fake);

            await sender.SendEmailAsync("test@test.com", "Subject", "<b>Hello</b>");

            Assert.That(fake.Sent.Count, Is.EqualTo(1));

            var sent = fake.Sent.First();
            Assert.That(sent.to, Is.EqualTo("test@test.com"));
            Assert.That(sent.subject, Is.EqualTo("Subject"));
            Assert.That(sent.body, Is.EqualTo("<b>Hello</b>"));
        }

        [Test]
        public async Task SendEmailAsync_ShouldForwardMultipleCalls()
        {
            var fake = new FakeEmailQueue();
            var sender = new EmailSenderService(fake);

            await sender.SendEmailAsync("a@test.com", "S1", "B1");
            await sender.SendEmailAsync("b@test.com", "S2", "B2");

            Assert.That(fake.Sent.Count, Is.EqualTo(2));
        }
    }
}

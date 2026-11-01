using FootballProjectSoftUni.Core.Extensions;
using FootballProjectSoftUni.Core.Models.Player;
using FootballProjectSoftUni.Core.Models.Team;
using FootballProjectSoftUni.Core.Services.Team;
using FootballProjectSoftUni.Core.Services.Tournament;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.Extensions.Localization;

namespace FootballProjectSoftUni.Tests.UnitTests
{
    /// <summary>
    /// Only the services' own messages may reach the screen, never the technical text of another exception.
    /// </summary>
    public class ServiceMessageLocalizerTests : UnitTestsBase
    {
        // Returns the resource key (and its arguments), so a test can see which text was chosen.
        private class KeyLocalizer : IStringLocalizer
        {
            public LocalizedString this[string name] => new(name, name);

            public LocalizedString this[string name, params object[] arguments] => new(name, name + ":" + string.Join(",", arguments));

            public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => Enumerable.Empty<LocalizedString>();
        }

        private readonly IStringLocalizer localizer = new KeyLocalizer();

        [Test]
        public void KnownServiceMessage_TranslatesTheServicesOwnMessages()
        {
            Assert.That(localizer.KnownServiceMessage("A Team with the same name already exists"), Is.EqualTo("SMTeamNameTaken"));
        }

        [TestCase("The instance of entity type 'Team' cannot be tracked because another instance with the key value '{Id: 5}' is already being tracked.")]
        [TestCase("Sequence contains no elements")]
        [TestCase("")]
        [TestCase(null)]
        public void KnownServiceMessage_IsNullForAnyOtherText(string? message)
        {
            Assert.That(localizer.KnownServiceMessage(message), Is.Null);
        }

        [Test]
        public void ServiceMessage_ShowsAnUnknownMessageOfAServiceResultAsItIs()
        {
            Assert.That(localizer.ServiceMessage("Some new service message."), Is.EqualTo("Some new service message."));
        }

        [Test]
        public async Task KnownServiceMessage_KnowsTheTeamMessagesWithANumber()
        {
            _data.Coaches.Add(new Coach { Id = "number-coach", Name = "Coach", Experience = "3" });
            await _data.SaveChangesAsync();
            var teamService = new TeamService(_data, new TournamentService(_data));

            var tooMany = new TeamRegistrationViewModel
            {
                TeamName = "Too many",
                Players = Enumerable.Range(1, 11).Select(i => new PlayerViewModel { Name = $"P{i}" }).ToList()
            };
            var tooLong = new TeamRegistrationViewModel
            {
                TeamName = "Too long",
                Players = Enumerable.Range(1, 6).Select(i => new PlayerViewModel { Name = new string('x', 51) }).ToList()
            };

            // The real messages of the service, so the translation can't drift away from them.
            var tooManyError = Assert.ThrowsAsync<InvalidOperationException>(() => teamService.CreateTeamDraftAsync(tooMany, "number-coach"));
            var tooLongError = Assert.ThrowsAsync<InvalidOperationException>(() => teamService.CreateTeamDraftAsync(tooLong, "number-coach"));

            Assert.That(localizer.KnownServiceMessage(tooManyError!.Message), Is.EqualTo("SMTooManyPlayers:10"));
            Assert.That(localizer.KnownServiceMessage(tooLongError!.Message), Is.EqualTo("SMPlayerNameTooLong:50"));
        }
    }
}

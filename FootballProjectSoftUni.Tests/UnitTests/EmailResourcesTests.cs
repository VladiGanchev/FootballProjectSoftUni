using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace FootballProjectSoftUni.Tests.UnitTests
{
    /// <summary>
    /// The automatic e-mails were once sent empty: their texts were blank in the resource files.
    /// </summary>
    public class EmailResourcesTests
    {
        // resource key -> number of {n} placeholders the code fills in
        private static readonly (string Key, int Args)[] EmailTexts =
        {
            ("PaymentSuccessSubject", 0),
            ("PaymentSuccessBody", 4),
            ("RefundSuccessSubject", 0),
            ("RefundSuccessBody", 3),
            ("RefundSuccessBodyShort", 1),
            ("TournamentCanceledSubject", 0),
            ("TournamentCanceledEmailBody", 3),
            ("EmailConfirmSubject", 0),
            ("EmailConfirmBody", 1),
            ("PasswordResetSubject", 0),
            ("PasswordResetBody", 1),
            ("NewTournamentSubject", 1),
            ("NewTournamentNotification", 2),
            ("NewTournamentEmailBody", 4),
            ("TournamentMovedSubject", 0),
            ("TournamentMovedNotification", 3),
            ("TournamentMovedEmailBody", 4)
        };

        [TestCase("bg")]
        [TestCase("en")]
        public void EmailTexts_AreFilledIn_AndUseTheirPlaceholders(string language)
        {
            var values = ReadResources(language);

            foreach (var (key, args) in EmailTexts)
            {
                Assert.That(values.ContainsKey(key), Is.True, $"{language}: {key} is missing");

                var text = values[key];
                Assert.That(string.IsNullOrWhiteSpace(text), Is.False, $"{language}: {key} is empty");

                var filled = string.Format(text, Enumerable.Range(0, args).Select(i => (object)$"<arg{i}>").ToArray());
                for (var i = 0; i < args; i++)
                {
                    Assert.That(filled, Does.Contain($"<arg{i}>"), $"{language}: {key} does not use {{{i}}}");
                }
            }
        }

        private static Dictionary<string, string> ReadResources(string language, [CallerFilePath] string thisFile = "")
        {
            var repo = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(thisFile)!, "..", ".."));
            var file = Path.Combine(repo, "FootballProjectSoftUni", "Resources", $"SharedResource.{language}.resx");

            return XDocument.Load(file).Root!
                .Elements("data")
                .ToDictionary(d => (string)d.Attribute("name")!, d => (string?)d.Element("value") ?? string.Empty);
        }
    }
}

using FootballProjectSoftUni.Core.Extensions;
using System.Globalization;

namespace FootballProjectSoftUni.Tests.UnitTests
{
    public class FormDatesTests
    {
        [TestCase("2026-11-03T18:00")]     // date-time picker
        [TestCase("2026-11-03T18:00:00")]
        [TestCase("03/11/2026 18:00")]     // older text format
        [TestCase("03.11.2026 18:00")]     // what the Bulgarian page used to fill in
        [TestCase("3.11.2026 18:00")]
        public void TryParseDateTime_AcceptsPickerAndOlderFormats(string text)
        {
            Assert.IsTrue(FormDates.TryParseDateTime(text, out var result));
            Assert.That(result, Is.EqualTo(new DateTime(2026, 11, 3, 18, 0, 0)));
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("31.02.2026 18:00")]
        [TestCase("tomorrow")]
        public void TryParseDateTime_RejectsInvalidValues(string? text)
        {
            Assert.IsFalse(FormDates.TryParseDateTime(text, out _));
        }

        [TestCase("1990-04-15")]           // date picker
        [TestCase("15.04.1990")]
        [TestCase("15/04/1990")]
        [TestCase("15/04/1990 00:00")]     // the old referee form asked for a time too
        public void TryParseDate_AcceptsPickerAndOlderFormats(string text)
        {
            Assert.IsTrue(FormDates.TryParseDate(text, out var result));
            Assert.That(result, Is.EqualTo(new DateTime(1990, 4, 15)));
        }

        [Test]
        public void InputValues_DoNotDependOnTheCurrentLanguage()
        {
            var previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("bg-BG");
                var date = new DateTime(2026, 11, 3, 18, 5, 0);

                Assert.That(date.ToDateTimeInput(), Is.EqualTo("2026-11-03T18:05"));
                Assert.That(date.ToDateInput(), Is.EqualTo("2026-11-03"));
            }
            finally
            {
                CultureInfo.CurrentCulture = previous;
            }
        }
    }
}

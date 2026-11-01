using FootballProjectSoftUni.Core.Extensions;

namespace FootballProjectSoftUni.Tests.UnitTests
{
    /// <summary>The site keeps Bulgarian time even on a server in another time zone (the hosting runs an hour behind).</summary>
    public class BgTimeTests
    {
        [Test]
        public void FromUtc_InWinter_IsTwoHoursAhead()
        {
            Assert.That(BgTime.FromUtc(new DateTime(2026, 1, 15, 10, 0, 0)), Is.EqualTo(new DateTime(2026, 1, 15, 12, 0, 0)));
        }

        [Test]
        public void FromUtc_InSummer_IsThreeHoursAhead()
        {
            Assert.That(BgTime.FromUtc(new DateTime(2026, 7, 15, 10, 0, 0)), Is.EqualTo(new DateTime(2026, 7, 15, 13, 0, 0)));
        }

        [Test]
        public void FromUtc_FollowsTheSwitchToSummerTime()
        {
            // 29 March 2026: at 03:00 Bulgarian time the clocks jump to 04:00 (01:00 UTC).
            Assert.That(BgTime.FromUtc(new DateTime(2026, 3, 29, 0, 59, 0)), Is.EqualTo(new DateTime(2026, 3, 29, 2, 59, 0)));
            Assert.That(BgTime.FromUtc(new DateTime(2026, 3, 29, 1, 0, 0)), Is.EqualTo(new DateTime(2026, 3, 29, 4, 0, 0)));
        }

        [Test]
        public void Now_IsTheCurrentMomentInBulgarianTime()
        {
            var expected = BgTime.FromUtc(DateTime.UtcNow);

            Assert.That(BgTime.Now, Is.EqualTo(expected).Within(TimeSpan.FromSeconds(5)));
            Assert.That(BgTime.Today, Is.EqualTo(BgTime.Now.Date));
        }
    }
}

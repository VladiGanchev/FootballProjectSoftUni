using FootballProjectSoftUni.Core.Extensions;

namespace FootballProjectSoftUni.Tests.UnitTests
{
    /// <summary>"?page=0", a negative or a huge page used to end with an error page.</summary>
    public class PagingTests
    {
        [TestCase(null, 1)]
        [TestCase(0, 1)]
        [TestCase(-5, 1)]
        [TestCase(1, 1)]
        [TestCase(7, 7)]
        [TestCase(int.MaxValue, Paging.MaxPage)]
        public void ValidPage_IsAPageTheListCanShow(int? page, int expected)
        {
            Assert.That(Paging.ValidPage(page), Is.EqualTo(expected));
        }
    }
}

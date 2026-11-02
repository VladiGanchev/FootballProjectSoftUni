using FootballProjectSoftUni.Core.Extensions;

namespace FootballProjectSoftUni.Tests.UnitTests
{
    /// <summary>Browsers keep files for a year only when a new version is sure to get a new address.</summary>
    public class StaticFileCacheTests
    {
        [Test]
        public void VersionedFile_IsKeptForAYear()
        {
            Assert.That(StaticFileCache.For("/css/site.css", hasQuery: true), Is.EqualTo(StaticFileCache.Year));
        }

        [Test]
        public void UploadedPicture_IsKeptForAYear()
        {
            Assert.That(StaticFileCache.For("/images/covers/cities/0a1b.jpg", hasQuery: false), Is.EqualTo(StaticFileCache.Year));
            Assert.That(StaticFileCache.For("/images/tournaments/36/0a1b.webp", hasQuery: false), Is.EqualTo(StaticFileCache.Year));
        }

        [Test]
        public void Font_IsKeptForAMonth()
        {
            Assert.That(StaticFileCache.For("/lib/fonts/onest-cyrillic.woff2", hasQuery: false), Is.EqualTo(StaticFileCache.Month));
        }

        [Test]
        public void UnversionedScriptOrStyle_IsCheckedEveryTime()
        {
            Assert.That(StaticFileCache.For("/js/site.js", hasQuery: false), Is.EqualTo(StaticFileCache.AskEveryTime));
            Assert.That(StaticFileCache.For("/css/site.css", hasQuery: false), Is.EqualTo(StaticFileCache.AskEveryTime));
            Assert.That(StaticFileCache.For("/favicon.ico", hasQuery: false), Is.EqualTo(StaticFileCache.AskEveryTime));
        }
    }
}

using FootballProjectSoftUni.Core.Services.Gallery;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace FootballProjectSoftUni.Tests.UnitTests
{
    /// <summary>The pictures of cities, tournaments and partners are kept by the site (wwwroot/images/covers).</summary>
    public class CoverPictureStoreTests
    {
        private string webRoot = null!;
        private CoverPictureStore store = null!;

        [SetUp]
        public void SetUp()
        {
            webRoot = Path.Combine(Path.GetTempPath(), "cover-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(webRoot);
            store = new CoverPictureStore(webRoot);
        }

        [TearDown]
        public void TearDown() => Directory.Delete(webRoot, recursive: true);

        [Test]
        public async Task SaveAsync_KeepsThePictureUnderARandomName_InTheFolderOfItsKind()
        {
            var url = await store.SaveAsync(await PngAsync(), "My city photo.PNG", CoverKind.Cities);

            Assert.That(url, Does.StartWith("/images/covers/cities/"));
            Assert.That(url, Does.EndWith(".png"));
            Assert.That(url, Does.Not.Contain("My city"));
            Assert.That(File.Exists(PathOf(url)), Is.True);
        }

        [Test]
        public async Task SaveAsync_GivesJpegFilesTheJpgExtension()
        {
            var photo = new MemoryStream();
            using (var image = new Image<Rgb24>(300, 200))
            {
                await image.SaveAsJpegAsync(photo);
            }
            photo.Position = 0;

            var url = await store.SaveAsync(photo, "poster.jpeg", CoverKind.Tournaments);

            Assert.That(url, Does.StartWith("/images/covers/tournaments/"));
            Assert.That(url, Does.EndWith(".jpg"));
            Assert.That(File.Exists(PathOf(url)), Is.True);
        }

        [Test]
        public void SaveAsync_LeavesNothingBehind_WhenThePictureCannotBeProcessed()
        {
            var broken = new MemoryStream(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3 });

            Assert.That(async () => await store.SaveAsync(broken, "logo.png", CoverKind.Partners), Throws.Exception);

            var folder = Path.Combine(webRoot, "images", "covers", "partners");
            Assert.That(Directory.Exists(folder) ? Directory.GetFiles(folder) : Array.Empty<string>(), Is.Empty);
        }

        [Test]
        public async Task Delete_RemovesAPictureOfTheSite()
        {
            var url = await store.SaveAsync(await PngAsync(), "logo.png", CoverKind.Partners);

            store.Delete(url);

            Assert.That(File.Exists(PathOf(url)), Is.False);
        }

        [Test]
        public void Delete_LeavesEverythingElseAlone()
        {
            // A file of the site outside the covers folder.
            var other = Path.Combine(webRoot, "images", "keep.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(other)!);
            File.WriteAllText(other, "keep");

            Assert.DoesNotThrow(() => store.Delete(null));
            Assert.DoesNotThrow(() => store.Delete(""));
            Assert.DoesNotThrow(() => store.Delete("https://example.com/images/covers/cities/x.jpg"));
            Assert.DoesNotThrow(() => store.Delete("/images/covers/../keep.txt"));
            Assert.DoesNotThrow(() => store.Delete("/images/covers/cities/../../keep.txt"));
            Assert.DoesNotThrow(() => store.Delete("/images/covers/cities/missing.jpg"));

            Assert.That(File.Exists(other), Is.True);
        }

        // ---------- Small copies for the lists ----------

        [Test]
        public void Small_IsTheCopyNextToAPictureOfTheSite_OtherAddressesStayTheSame()
        {
            Assert.That(CoverPictureStore.Small("/images/covers/cities/abc.jpg"), Is.EqualTo("/images/covers/cities/abc-sm.jpg"));
            Assert.That(CoverPictureStore.Small("https://example.com/sofia.jpg"), Is.EqualTo("https://example.com/sofia.jpg"));
            Assert.That(CoverPictureStore.Small(null), Is.Null);
            Assert.That(CoverPictureStore.Small(""), Is.EqualTo(""));
        }

        [Test]
        public async Task SaveAsync_AlsoKeepsASmallCopy_AtMost640px()
        {
            var big = new MemoryStream();
            using (var image = new Image<Rgb24>(1600, 1000))
            {
                await image.SaveAsJpegAsync(big);
            }
            big.Position = 0;

            var url = await store.SaveAsync(big, "big.jpg", CoverKind.Cities);
            var small = PathOf(CoverPictureStore.Small(url)!);

            Assert.That(File.Exists(small), Is.True);
            using var copy = await Image.LoadAsync(small);
            Assert.That(copy.Width, Is.EqualTo(640));
            Assert.That(copy.Height, Is.EqualTo(400));
        }

        [Test]
        public async Task Delete_AlsoRemovesTheSmallCopy()
        {
            var url = await store.SaveAsync(await PngAsync(), "logo.png", CoverKind.Partners);

            store.Delete(url);

            Assert.That(File.Exists(PathOf(CoverPictureStore.Small(url)!)), Is.False);
        }

        [Test]
        public async Task CreateMissingSmallCopies_MakesThemOnlyWhereTheyAreMissing()
        {
            var url = await store.SaveAsync(await PngAsync(), "a.png", CoverKind.Cities);
            var older = await store.SaveAsync(await PngAsync(), "b.png", CoverKind.Tournaments);
            File.Delete(PathOf(CoverPictureStore.Small(older)!));                     // saved before there were copies
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(PathOf(url))!, "broken.png"), "not a picture");

            var made = await store.CreateMissingSmallCopiesAsync();

            Assert.That(made, Is.EqualTo(1));
            Assert.That(File.Exists(PathOf(CoverPictureStore.Small(older)!)), Is.True);
            Assert.That(await store.CreateMissingSmallCopiesAsync(), Is.EqualTo(0), "nothing is made twice; a broken file is skipped");
        }

        private string PathOf(string url) =>
            Path.Combine(webRoot, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

        private static async Task<MemoryStream> PngAsync()
        {
            var picture = new MemoryStream();
            using (var image = new Image<Rgba32>(320, 160))
            {
                await image.SaveAsPngAsync(picture);
            }
            picture.Position = 0;
            return picture;
        }
    }
}

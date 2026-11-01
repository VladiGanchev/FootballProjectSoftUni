using FootballProjectSoftUni.Core.Services.Gallery;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;

namespace FootballProjectSoftUni.Tests.UnitTests
{
    /// <summary>Gallery pictures are public: they must not carry the phone's hidden data (GPS position...).</summary>
    public class GalleryPictureTests
    {
        private string folder = null!;

        [SetUp]
        public void SetUp()
        {
            folder = Path.Combine(Path.GetTempPath(), "gallery-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
        }

        [TearDown]
        public void TearDown() => Directory.Delete(folder, recursive: true);

        [Test]
        public async Task SaveAsync_RemovesTheHiddenData_TurnsThePictureAndMakesItSmaller()
        {
            // A 4000 x 3000 phone photo, held sideways (orientation 6 = turn 90 degrees), with a GPS position.
            var photo = new MemoryStream();
            using (var image = new Image<Rgb24>(4000, 3000))
            {
                var exif = new ExifProfile();
                exif.SetValue(ExifTag.Make, "TestPhone");
                exif.SetValue(ExifTag.Orientation, (ushort)6);
                exif.SetValue(ExifTag.GPSLatitudeRef, "N");
                exif.SetValue(ExifTag.GPSLatitude, new[] { new Rational(42, 1), new Rational(41, 1), new Rational(30, 1) });
                image.Metadata.ExifProfile = exif;
                await image.SaveAsJpegAsync(photo);
            }
            photo.Position = 0;

            var path = Path.Combine(folder, "photo.jpg");
            await GalleryPicture.SaveAsync(photo, path);

            using var saved = await Image.LoadAsync(path);
            Assert.That(saved.Metadata.ExifProfile, Is.Null);
            Assert.That(saved.Width, Is.EqualTo(1440));
            Assert.That(saved.Height, Is.EqualTo(GalleryPicture.MaxSide));
        }

        [Test]
        public async Task SaveAsync_KeepsASmallPictureAsItIs()
        {
            var picture = new MemoryStream();
            using (var image = new Image<Rgba32>(800, 600))
            {
                await image.SaveAsPngAsync(picture);
            }
            picture.Position = 0;

            var path = Path.Combine(folder, "small.png");
            await GalleryPicture.SaveAsync(picture, path);

            var info = await Image.IdentifyAsync(path);
            Assert.That((info.Width, info.Height), Is.EqualTo((800, 600)));
        }

        [Test]
        public async Task CanTakeAsync_RefusesAFileThatIsNotAPicture()
        {
            // Starts like a JPEG, but is not one.
            var fake = new MemoryStream(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }.Concat(System.Text.Encoding.ASCII.GetBytes("<script>alert(1)</script>")).ToArray());

            Assert.That(await GalleryPicture.CanTakeAsync(fake), Is.False);
        }

        [Test]
        public async Task CanTakeAsync_RefusesAPictureWithTooManyPixels_WithoutOpeningIt()
        {
            // Just a PNG header that claims 10000 x 10000 pixels (a "decompression bomb").
            var bomb = new MemoryStream(PngHeader(10_000, 10_000));
            var normal = new MemoryStream(PngHeader(1920, 1080));

            Assert.That(await GalleryPicture.CanTakeAsync(bomb), Is.False);
            Assert.That(await GalleryPicture.CanTakeAsync(normal), Is.True);
        }

        [Test]
        public async Task CheckAsync_TakesOnlyRealPicturesOfTheAllowedTypes()
        {
            var png = PngHeader(800, 600);
            Func<Stream> Open(byte[] bytes) => () => new MemoryStream(bytes);

            Assert.That(await GalleryPicture.CheckAsync("city.png", png.Length, Open(png)), Is.EqualTo(PictureProblem.None));
            Assert.That(await GalleryPicture.CheckAsync("CITY.PNG", png.Length, Open(png)), Is.EqualTo(PictureProblem.None));

            Assert.That(await GalleryPicture.CheckAsync("city.svg", png.Length, Open(png)), Is.EqualTo(PictureProblem.NotAllowedType));
            Assert.That(await GalleryPicture.CheckAsync("city", png.Length, Open(png)), Is.EqualTo(PictureProblem.NotAllowedType));
            Assert.That(await GalleryPicture.CheckAsync("city.png", 0, Open(png)), Is.EqualTo(PictureProblem.Empty));
            Assert.That(await GalleryPicture.CheckAsync("city.png", GalleryPicture.MaxFileBytes + 1, Open(png)), Is.EqualTo(PictureProblem.TooLarge));

            // A PNG named .jpg, a page named .png, a WAVE sound named .webp.
            var page = System.Text.Encoding.ASCII.GetBytes("<html><script>alert(1)</script></html>");
            var wave = System.Text.Encoding.ASCII.GetBytes("RIFF\0\0\0\0WAVEfmt ");
            Assert.That(await GalleryPicture.CheckAsync("city.jpg", png.Length, Open(png)), Is.EqualTo(PictureProblem.NotAPicture));
            Assert.That(await GalleryPicture.CheckAsync("city.png", page.Length, Open(page)), Is.EqualTo(PictureProblem.NotAPicture));
            Assert.That(await GalleryPicture.CheckAsync("city.webp", wave.Length, Open(wave)), Is.EqualTo(PictureProblem.NotAPicture));

            // Too many pixels.
            var bomb = PngHeader(10_000, 10_000);
            Assert.That(await GalleryPicture.CheckAsync("city.png", bomb.Length, Open(bomb)), Is.EqualTo(PictureProblem.NotAPicture));
        }

        private static byte[] PngHeader(int width, int height)
        {
            var ihdr = new List<byte>();
            ihdr.AddRange(BigEndian(width));
            ihdr.AddRange(BigEndian(height));
            ihdr.AddRange(new byte[] { 8, 2, 0, 0, 0 });   // 8 bits, RGB

            var chunk = new List<byte>();
            chunk.AddRange(BigEndian(13));
            var typeAndData = System.Text.Encoding.ASCII.GetBytes("IHDR").Concat(ihdr).ToArray();
            chunk.AddRange(typeAndData);
            chunk.AddRange(BigEndian((int)Crc32(typeAndData)));

            return new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }.Concat(chunk).ToArray();
        }

        private static byte[] BigEndian(int value) =>
            new[] { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value };

        private static uint Crc32(byte[] data)
        {
            var crc = 0xFFFFFFFFu;
            foreach (var b in data)
            {
                crc ^= b;
                for (var k = 0; k < 8; k++)
                {
                    crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320u : crc >> 1;
                }
            }

            return ~crc;
        }
    }
}

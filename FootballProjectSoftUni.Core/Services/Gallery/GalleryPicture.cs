using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace FootballProjectSoftUni.Core.Services.Gallery
{
    /// <summary>
    /// A picture as the site keeps it (gallery, and the pictures of cities, tournaments and partners): the right
    /// way up, at most 1920 px on its longer side, and without the hidden data phones put into photos (GPS
    /// position, phone model, date...). Saving the picture anew also leaves out anything that only pretends to be
    /// an image.
    /// </summary>
    public static class GalleryPicture
    {
        public const int MaxSide = 1920;

        /// <summary>The longer side of the small copy shown in lists.</summary>
        public const int SmallSide = 640;

        // Checked before the picture is opened: a small file can unpack into gigabytes of pixels.
        public const long MaxPixels = 50_000_000;

        public const long MaxFileBytes = 8 * 1024 * 1024;

        // Pictures end up in wwwroot and are served from the site's own domain, so only real images are taken:
        // a whitelisted extension with the matching file signature.
        private static readonly Dictionary<string, byte[][]> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] = new[] { new byte[] { 0xFF, 0xD8, 0xFF } },
            [".jpeg"] = new[] { new byte[] { 0xFF, 0xD8, 0xFF } },
            [".png"] = new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } },
            [".gif"] = new[] { new byte[] { 0x47, 0x49, 0x46, 0x38 } },
            [".webp"] = new[] { new byte[] { 0x52, 0x49, 0x46, 0x46 } } // "RIFF", "WEBP" checked below
        };

        public static bool IsAllowedExtension(string? extension) =>
            !string.IsNullOrEmpty(extension) && AllowedTypes.ContainsKey(extension);

        /// <summary>Whether an uploaded file can become a picture of the site (type, size, a real picture).</summary>
        public static async Task<PictureProblem> CheckAsync(string fileName, long length, Func<Stream> open)
        {
            var extension = Path.GetExtension(fileName);

            if (string.IsNullOrEmpty(extension) || !AllowedTypes.TryGetValue(extension, out var signatures))
            {
                return PictureProblem.NotAllowedType;
            }

            if (length == 0)
            {
                return PictureProblem.Empty;
            }

            if (length > MaxFileBytes)
            {
                return PictureProblem.TooLarge;
            }

            var header = new byte[12];
            int read;
            using (var stream = open())
            {
                read = await stream.ReadAsync(header, 0, header.Length);
            }

            var matches = signatures.Any(sig => read >= sig.Length && header.Take(sig.Length).SequenceEqual(sig));

            if (matches && extension.Equals(".webp", StringComparison.OrdinalIgnoreCase))
            {
                // RIFF....WEBP
                matches = read >= 12 && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50;
            }

            if (!matches)
            {
                return PictureProblem.NotAPicture;
            }

            using (var stream = open())
            {
                return await CanTakeAsync(stream) ? PictureProblem.None : PictureProblem.NotAPicture;
            }
        }

        /// <summary>Whether this is a picture the site can take (reads only its header).</summary>
        public static async Task<bool> CanTakeAsync(Stream input)
        {
            try
            {
                var info = await Image.IdentifyAsync(input);
                return (long)info.Width * info.Height <= MaxPixels;
            }
            catch (Exception e) when (e is ImageFormatException or NotSupportedException)
            {
                return false;
            }
        }

        /// <summary>Saves the picture to <paramref name="path"/>, in the format of its extension.</summary>
        public static async Task SaveAsync(Stream input, string path)
        {
            using var image = await Image.LoadAsync(input);

            // Phones keep the rotation in the hidden data, so it is applied before that data goes.
            image.Mutate(x => x.AutoOrient());

            if (image.Width > MaxSide || image.Height > MaxSide)
            {
                image.Mutate(x => x.Resize(new ResizeOptions { Mode = ResizeMode.Max, Size = new Size(MaxSide, MaxSide) }));
            }

            image.Metadata.ExifProfile = null;
            image.Metadata.XmpProfile = null;
            image.Metadata.IptcProfile = null;
            image.Metadata.GetPngMetadata().TextData.Clear();
            image.Metadata.GetGifMetadata().Comments.Clear();

            foreach (var frame in image.Frames)
            {
                frame.Metadata.ExifProfile = null;
                frame.Metadata.XmpProfile = null;
                frame.Metadata.IptcProfile = null;
            }

            await image.SaveAsync(path, EncoderFor(path));
        }

        /// <summary>
        /// A small copy (at most <see cref="SmallSide"/> px) of a picture already saved by <see cref="SaveAsync"/>,
        /// for lists, where the picture is shown a few hundred pixels wide: a fraction of the download on a phone.
        /// </summary>
        public static async Task SaveSmallCopyAsync(string path, string smallPath)
        {
            using var image = await Image.LoadAsync(path);

            if (image.Width > SmallSide || image.Height > SmallSide)
            {
                image.Mutate(x => x.Resize(new ResizeOptions { Mode = ResizeMode.Max, Size = new Size(SmallSide, SmallSide) }));
            }

            await image.SaveAsync(smallPath, EncoderFor(smallPath));
        }

        private static IImageEncoder EncoderFor(string path) => Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".png" => new PngEncoder(),
            ".gif" => new GifEncoder(),
            ".webp" => new WebpEncoder { Quality = 85 },
            _ => new JpegEncoder { Quality = 85 }
        };
    }
}

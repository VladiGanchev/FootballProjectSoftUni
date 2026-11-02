namespace FootballProjectSoftUni.Core.Services.Gallery
{
    public enum CoverKind
    {
        Cities,
        Tournaments,
        Partners
    }

    /// <summary>
    /// The picture of a city, a tournament or a partner, kept by the site itself (wwwroot/images/covers) instead of
    /// a link to another site: visitors' browsers contact nobody else, and the picture can't change or disappear.
    /// Next to every picture there is a small copy ("name-sm.jpg") for the lists.
    /// </summary>
    public class CoverPictureStore
    {
        public const string UrlRoot = "/images/covers/";
        public const string SmallSuffix = "-sm";

        private readonly string root;

        public CoverPictureStore(string webRootPath)
        {
            root = Path.GetFullPath(Path.Combine(webRootPath, "images", "covers"));
        }

        /// <summary>
        /// The address of the small copy of a picture kept by the site; any other address (e.g. an old link to
        /// another site, which has no copy) comes back unchanged.
        /// </summary>
        public static string? Small(string? url)
        {
            if (string.IsNullOrWhiteSpace(url) || !url.StartsWith(UrlRoot, StringComparison.Ordinal))
            {
                return url;
            }

            var extension = Path.GetExtension(url);
            return extension.Length == 0 ? url : url.Substring(0, url.Length - extension.Length) + SmallSuffix + extension;
        }

        /// <summary>Processes the picture (GalleryPicture) and saves it under a random name, never the uploader's.</summary>
        /// <returns>The address of the saved picture.</returns>
        public async Task<string> SaveAsync(Stream input, string originalFileName, CoverKind kind)
        {
            var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
            if (extension == ".jpeg") extension = ".jpg";

            var folder = kind.ToString().ToLowerInvariant();
            Directory.CreateDirectory(Path.Combine(root, folder));

            var name = $"{Guid.NewGuid():N}{extension}";
            var path = Path.Combine(root, folder, name);
            var smallPath = SmallPathOf(path);

            try
            {
                await GalleryPicture.SaveAsync(input, path);
                await GalleryPicture.SaveSmallCopyAsync(path, smallPath);
            }
            catch
            {
                if (File.Exists(path)) File.Delete(path);
                if (File.Exists(smallPath)) File.Delete(smallPath);
                throw;
            }

            return UrlRoot + folder + "/" + name;
        }

        /// <summary>
        /// Makes the small copies that are missing (pictures saved before there were copies).
        /// A picture that can't be read is skipped: the lists then show the picture itself.
        /// </summary>
        /// <returns>How many copies were made.</returns>
        public async Task<int> CreateMissingSmallCopiesAsync()
        {
            if (!Directory.Exists(root))
            {
                return 0;
            }

            var made = 0;
            foreach (var path in Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories))
            {
                if (Path.GetFileNameWithoutExtension(path).EndsWith(SmallSuffix, StringComparison.Ordinal)
                    || !GalleryPicture.IsAllowedExtension(Path.GetExtension(path)))
                {
                    continue;
                }

                var smallPath = SmallPathOf(path);
                if (File.Exists(smallPath))
                {
                    continue;
                }

                try
                {
                    await GalleryPicture.SaveSmallCopyAsync(path, smallPath);
                    made++;
                }
                catch
                {
                    if (File.Exists(smallPath)) File.Delete(smallPath);
                }
            }

            return made;
        }

        /// <summary>Deletes a picture kept by the site and its small copy; anything else (e.g. an old link to another site) is left alone.</summary>
        public void Delete(string? url)
        {
            if (string.IsNullOrWhiteSpace(url) || !url.StartsWith(UrlRoot, StringComparison.Ordinal))
            {
                return;
            }

            var path = Path.GetFullPath(Path.Combine(root, url.Substring(UrlRoot.Length).Replace('/', Path.DirectorySeparatorChar)));

            // Never outside the covers folder, whatever the address says ("../" ...).
            if (!path.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            foreach (var file in new[] { path, SmallPathOf(path) })
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
        }

        private static string SmallPathOf(string path) =>
            Path.Combine(Path.GetDirectoryName(path)!, Path.GetFileNameWithoutExtension(path) + SmallSuffix + Path.GetExtension(path));
    }
}

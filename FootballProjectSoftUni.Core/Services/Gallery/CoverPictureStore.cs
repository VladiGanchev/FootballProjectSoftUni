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
    /// </summary>
    public class CoverPictureStore
    {
        public const string UrlRoot = "/images/covers/";

        private readonly string root;

        public CoverPictureStore(string webRootPath)
        {
            root = Path.GetFullPath(Path.Combine(webRootPath, "images", "covers"));
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

            try
            {
                await GalleryPicture.SaveAsync(input, path);
            }
            catch
            {
                if (File.Exists(path)) File.Delete(path);
                throw;
            }

            return UrlRoot + folder + "/" + name;
        }

        /// <summary>Deletes a picture kept by the site; anything else (e.g. an old link to another site) is left alone.</summary>
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

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}

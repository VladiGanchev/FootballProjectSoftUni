using FootballProjectSoftUni.Core.Services.Gallery;
using Microsoft.Extensions.Localization;

namespace FootballProjectSoftUni.Extensions
{
    public static class UploadedPicture
    {
        /// <summary>What the file pickers offer (the server checks again: GalleryPicture.CheckAsync).</summary>
        public const string Accept = "image/jpeg,image/png,image/webp,image/gif";

        /// <summary>Why an uploaded file can't become a picture of the site, in the visitor's language; null when it can.</summary>
        public static async Task<string?> ProblemAsync(this IStringLocalizer localizer, IFormFile file)
        {
            var name = Path.GetFileName(file.FileName);

            return await GalleryPicture.CheckAsync(file.FileName, file.Length, file.OpenReadStream) switch
            {
                PictureProblem.None => null,
                PictureProblem.NotAllowedType => localizer["GLNotAllowed", name].Value,
                PictureProblem.Empty => localizer["GLEmpty", name].Value,
                PictureProblem.TooLarge => localizer["GLTooLarge", name, GalleryPicture.MaxFileBytes / (1024 * 1024)].Value,
                _ => localizer["GLNotAnImage", name].Value
            };
        }
    }
}

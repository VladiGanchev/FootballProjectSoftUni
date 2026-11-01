using FootballProjectSoftUni.Core.Services.Gallery;
using FootballProjectSoftUni.Extensions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;

namespace FootballProjectSoftUni.Services
{
    /// <summary>
    /// The picture field of the forms for cities, tournaments and partners (a file named "image"):
    /// checks it, saves it into the site (CoverPictureStore) and removes pictures that are no longer used.
    /// Problems go into the form errors under "image", next to the field.
    /// </summary>
    public class CoverPictureUpload
    {
        public const string FieldName = "image";

        private readonly CoverPictureStore store;
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly ILogger<CoverPictureUpload> logger;

        public CoverPictureUpload(CoverPictureStore store, IStringLocalizer<SharedResource> localizer, ILogger<CoverPictureUpload> logger)
        {
            this.store = store;
            this.localizer = localizer;
            this.logger = logger;
        }

        public async Task CheckAsync(IFormFile? file, ModelStateDictionary modelState, bool required)
        {
            if (file == null)
            {
                if (required)
                {
                    modelState.AddModelError(FieldName, localizer["PicRequired"].Value);
                }

                return;
            }

            var problem = await localizer.ProblemAsync(file);
            if (problem != null)
            {
                modelState.AddModelError(FieldName, problem);
            }
        }

        /// <summary>
        /// The form goes back because of another field: browsers don't keep a chosen file, so the admin is asked
        /// to choose the picture again.
        /// </summary>
        public void AskAgainIfChosen(IFormFile? file, ModelStateDictionary modelState)
        {
            if (file != null && modelState.GetFieldValidationState(FieldName) != ModelValidationState.Invalid)
            {
                modelState.AddModelError(FieldName, localizer["PicChooseAgain"].Value);
            }
        }

        /// <returns>The address of the saved picture, or null (with a form error) when it couldn't be processed.</returns>
        public async Task<string?> SaveAsync(IFormFile file, CoverKind kind, ModelStateDictionary modelState)
        {
            try
            {
                using var input = file.OpenReadStream();
                return await store.SaveAsync(input, file.FileName, kind);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Picture {FileName} ({Kind}) could not be processed", file.FileName, kind);
                modelState.AddModelError(FieldName, localizer["GLSaveFailed", Path.GetFileName(file.FileName)].Value);
                return null;
            }
        }

        /// <summary>Removes a picture of the site that is no longer used (old links to other sites are left alone).</summary>
        public void Delete(string? url)
        {
            try
            {
                store.Delete(url);
            }
            catch (Exception ex)
            {
                // The change is already saved; a file that stays behind only takes some space.
                logger.LogWarning(ex, "Picture {Url} could not be removed", url);
            }
        }
    }
}

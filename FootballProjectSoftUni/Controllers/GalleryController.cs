using FootballProjectSoftUni.Core.Services.Gallery;
using FootballProjectSoftUni.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace FootballProjectSoftUni.Controllers
{
    [Authorize]
    public class GalleryController : Controller
    {
        // Uploads end up in wwwroot and are served from the site's own domain, so only real
        // images are accepted (GalleryPicture.CheckAsync: whitelisted extension + matching file
        // signature, at most 8 MB), saved under a random file name (never the uploader's name).
        private const int MaxFilesPerUpload = 20;
        private const long MaxRequestBytes = 60 * 1024 * 1024;   // whole upload request

        private readonly IWebHostEnvironment env;
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly ILogger<GalleryController> logger;

        public GalleryController(IWebHostEnvironment env, IStringLocalizer<SharedResource> localizer, ILogger<GalleryController> logger)
        {
            this.env = env;
            this.localizer = localizer;
            this.logger = logger;
        }

        private string GalleryFolder(int id) =>
            Path.Combine(env.WebRootPath, "images", "tournaments", id.ToString());

        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public IActionResult UploadPictures(int id)
        {
            ViewBag.TournamentId = id;
            return View();
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        [RequestSizeLimit(MaxRequestBytes)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
        public async Task<IActionResult> UploadPictures(int id, List<IFormFile> files)
        {
            ViewBag.TournamentId = id;

            if (files == null || files.Count == 0)
            {
                ModelState.AddModelError("", localizer["GLSelectPictures"].Value);
                return View();
            }

            if (files.Count > MaxFilesPerUpload)
            {
                ModelState.AddModelError("", localizer["GLTooManyFiles", MaxFilesPerUpload].Value);
                return View();
            }

            // Validate everything first, so a bad file doesn't leave half an upload behind.
            foreach (var file in files)
            {
                var error = await localizer.ProblemAsync(file);
                if (error != null)
                {
                    ModelState.AddModelError("", error);
                    return View();
                }
            }

            string folderPath = GalleryFolder(id);
            Directory.CreateDirectory(folderPath);

            // Every picture is saved anew (GalleryPicture): turned the right way up, smaller, and without the
            // hidden data of the phone (GPS position...). If one of them can't be processed after all,
            // nothing of this upload stays.
            var saved = new List<string>();
            foreach (var file in files)
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (extension == ".jpeg") extension = ".jpg";

                string fileName = $"{Guid.NewGuid():N}{extension}";
                string filePath = Path.Combine(folderPath, fileName);

                try
                {
                    saved.Add(filePath);
                    using var input = file.OpenReadStream();
                    await GalleryPicture.SaveAsync(input, filePath);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Gallery picture {FileName} for tournament {TournamentId} could not be processed", file.FileName, id);
                    saved.Where(System.IO.File.Exists).ToList().ForEach(System.IO.File.Delete);

                    ModelState.AddModelError("", localizer["GLSaveFailed", Path.GetFileName(file.FileName)].Value);
                    return View();
                }
            }

            TempData["Success"] = localizer["GLUploaded"].Value;
            return RedirectToAction("TournamentGallery", new { id = id });
        }

        [AllowAnonymous]
        public IActionResult TournamentGallery(int id)
        {
            string folderPath = GalleryFolder(id);

            var images = Directory.Exists(folderPath)
                ? Directory.GetFiles(folderPath)
                    .Where(x => GalleryPicture.IsAllowedExtension(Path.GetExtension(x)))
                    .Select(x => $"/images/tournaments/{id}/{Path.GetFileName(x)}")
                    .ToList()
                : new List<string>();

            ViewBag.TournamentId = id;
            return View(images);
        }

        [HttpPost]
        public IActionResult DeletePicture(int id, string fileName)
        {
            if (!User.IsAdmin())
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return BadRequest();
            }

            fileName = Path.GetFileName(fileName);

            var folderPath = GalleryFolder(id);
            var filePath = Path.Combine(folderPath, fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                TempData["Success"] = localizer["GLDeleted"].Value;
            }
            else
            {
                TempData["Error"] = localizer["GLFileNotFound"].Value;
            }

            return RedirectToAction("TournamentGallery", new { id = id });
        }
    }
}

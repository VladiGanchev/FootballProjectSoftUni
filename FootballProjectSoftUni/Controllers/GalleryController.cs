using FootballProjectSoftUni.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballProjectSoftUni.Controllers
{
    [Authorize] 
    public class GalleryController : Controller
    {
        [Authorize(Roles = "Administrator")]
        [HttpGet]
        public IActionResult UploadPictures(int id)
        {
            ViewBag.TournamentId = id;
            return View();
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost]
        public async Task<IActionResult> UploadPictures(int id, List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
            {
                ModelState.AddModelError("", "Please select at least one picture.");
                ViewBag.TournamentId = id;
                return View();
            }

            string folderPath = Path.Combine("wwwroot", "images", "tournaments", id.ToString());

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            foreach (var file in files)
            {
                string fileName = Path.GetFileName(file.FileName);
                string filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }

            TempData["Success"] = "Pictures uploaded successfully!";
            return RedirectToAction("TournamentGallery", new { id = id });
        }

        [AllowAnonymous]
        public IActionResult TournamentGallery(int id)
        {
            string folderPath = Path.Combine("wwwroot", "images", "tournaments", id.ToString());

            var images = Directory.Exists(folderPath)
                ? Directory.GetFiles(folderPath).Select(x => $"/images/tournaments/{id}/{Path.GetFileName(x)}").ToList()
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

            // Малка защита: да няма пътеки вътре
            fileName = Path.GetFileName(fileName);

            var root = Directory.GetCurrentDirectory();
            var folderPath = Path.Combine(root, "wwwroot", "images", "tournaments", id.ToString());
            var filePath = Path.Combine(folderPath, fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                TempData["Success"] = "Picture deleted successfully.";
            }
            else
            {
                TempData["Error"] = "File not found.";
            }

            return RedirectToAction("TournamentGallery", new { id = id });
        }
    }
}

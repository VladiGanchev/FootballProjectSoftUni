using FootballProjectSoftUni.Core.Contracts.City;
using FootballProjectSoftUni.Core.Contracts.Partner;
using FootballProjectSoftUni.Core.Models.City;
using FootballProjectSoftUni.Core.Models.Partner;
using FootballProjectSoftUni.Core.Services.City;
using FootballProjectSoftUni.Core.Services.Gallery;
using FootballProjectSoftUni.Extensions;
using FootballProjectSoftUni.Models;
using FootballProjectSoftUni.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace FootballProjectSoftUni.Controllers
{
    public class PartnerController : Controller
    {
        private readonly IPartnerService service;
        private readonly CoverPictureUpload pictures;
        private readonly IStringLocalizer<SharedResource> localizer;

        public PartnerController(IPartnerService _service, CoverPictureUpload _pictures, IStringLocalizer<SharedResource> _localizer)
        {
            service = _service;
            pictures = _pictures;
            localizer = _localizer;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> All()
        {
            var partners = await service.AllPartnersAsync();
            return View(partners);
        }

        [HttpGet]
        public IActionResult AddPartner()
        {
            if (User.IsAdmin() == false)
            {
                return Unauthorized();
            }

            var model = new PartnerViewModel();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddPartner(PartnerViewModel model, IFormFile? image)
        {
            if (User.IsAdmin() == false)
            {
                return Unauthorized();
            }

            await pictures.CheckAsync(image, ModelState, required: true);

            if (ModelState.IsValid == false)
            {
                pictures.AskAgainIfChosen(image, ModelState);
                return View(model);
            }

            var imageUrl = await pictures.SaveAsync(image!, CoverKind.Partners, ModelState);
            if (imageUrl == null)
            {
                return View(model);
            }

            model.ImageUrl = imageUrl;

            try
            {
                await service.AddPartnerAsync(model);
            }
            catch
            {
                pictures.Delete(imageUrl);
                throw;
            }

            return RedirectToAction(nameof(All));
        }

        // A new logo for a partner; the old one is removed from the site.
        [HttpGet]
        public async Task<IActionResult> ChangePicture(int? id)
        {
            if (User.IsAdmin() == false)
            {
                return Unauthorized();
            }

            return View(await ChangePictureModelAsync(id));
        }

        [HttpPost]
        public async Task<IActionResult> ChangePicture(int? id, IFormFile? image)
        {
            if (User.IsAdmin() == false)
            {
                return Unauthorized();
            }

            var model = await ChangePictureModelAsync(id);

            if (model.Items.All(p => p.Id != id))
            {
                ModelState.AddModelError("id", localizer["PicPickPartner"].Value);
            }

            await pictures.CheckAsync(image, ModelState, required: true);

            if (ModelState.IsValid == false)
            {
                pictures.AskAgainIfChosen(image, ModelState);
                return View(model);
            }

            var imageUrl = await pictures.SaveAsync(image!, CoverKind.Partners, ModelState);
            if (imageUrl == null)
            {
                return View(model);
            }

            string? oldImageUrl;
            try
            {
                oldImageUrl = await service.ChangePartnerImageAsync(id!.Value, imageUrl);
            }
            catch
            {
                pictures.Delete(imageUrl);
                throw;
            }

            // Deleted in the meantime.
            if (oldImageUrl == null)
            {
                pictures.Delete(imageUrl);
                return NotFound();
            }

            pictures.Delete(oldImageUrl);

            TempData["PaymentMessage"] = localizer["PicLogoChanged"].Value;
            TempData["PaymentType"] = "success";

            return RedirectToAction(nameof(All));
        }

        private async Task<ChangePictureViewModel> ChangePictureModelAsync(int? id) => new()
        {
            Controller = "Partner",
            IsLogo = true,
            SelectedId = id,
            Items = (await service.AllPartnersAsync())
                .OrderBy(p => p.Name)
                .Select(p => new PictureOwner(p.Id, p.Name, p.ImageUrl))
                .ToList()
        };

        public async Task<IActionResult> DeletePartner()
        {
            if (User.IsAdmin() == false)
            {
                return Unauthorized();
            }

            var partners = await service.AllPartnersAsync();

            return View(partners);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteConfirmed(PartnerViewModel model, int id)
        {
            if (User.IsAdmin() == false)
            {
                return Unauthorized();
            }

            var partner = await service.FindPartnerAsync(model, id);

            if (partner == null)
            {
                return NotFound();
            }

            return View(partner);

        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (User.IsAdmin() == false)
            {
                return Unauthorized();
            }

            var partner = await service.FindPartnerAsync(new PartnerViewModel(), id);

            // The logo goes together with the partner.
            if (await service.DeletePartnerAsync(id) && partner != null)
            {
                pictures.Delete(partner.ImageUrl);
            }

            return RedirectToAction(nameof(All));
        }
    }
}

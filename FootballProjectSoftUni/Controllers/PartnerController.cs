using FootballProjectSoftUni.Core.Contracts.City;
using FootballProjectSoftUni.Core.Contracts.Partner;
using FootballProjectSoftUni.Core.Models.City;
using FootballProjectSoftUni.Core.Models.Partner;
using FootballProjectSoftUni.Core.Services.City;
using FootballProjectSoftUni.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FootballProjectSoftUni.Controllers
{
    public class PartnerController : Controller
    {
        private readonly IPartnerService service;
        public PartnerController(IPartnerService _service)
        {
            service = _service;
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
        public async Task<IActionResult> AddPartner(PartnerViewModel model)
        {
            if (User.IsAdmin() == false)
            {
                return Unauthorized();
            }

            if (ModelState.IsValid == false)
            {
                return View(model);
            }

            await service.AddPartnerAsync(model);

            return RedirectToAction(nameof(All));
        }

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

            await service.DeletePartnerAsync(id);

            return RedirectToAction(nameof(All));
        }
    }
}

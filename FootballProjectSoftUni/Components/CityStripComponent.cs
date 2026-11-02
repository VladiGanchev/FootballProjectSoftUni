using FootballProjectSoftUni.Core.Contracts.City;
using Microsoft.AspNetCore.Mvc;

namespace FootballProjectSoftUni.Components
{
    /// <summary>
    /// Read-only strip of all cities shown on the landing page (it goes round endlessly, see data-loop in site.js).
    /// Uses the existing city service; it does not change any data.
    /// </summary>
    public class CityStripComponent : ViewComponent
    {
        private readonly ICityService cityService;

        public CityStripComponent(ICityService _cityService)
        {
            cityService = _cityService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var cities = await cityService.AllCitiesAsync();

            return View(cities.ToList());
        }
    }
}

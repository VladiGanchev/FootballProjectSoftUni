using FootballProjectSoftUni.Core.Contracts.City;
using Microsoft.AspNetCore.Mvc;

namespace FootballProjectSoftUni.Components
{
    /// <summary>
    /// Read-only strip of cities shown on the landing page.
    /// Uses the existing city service; it does not change any data.
    /// </summary>
    public class CityStripComponent : ViewComponent
    {
        private readonly ICityService cityService;

        public CityStripComponent(ICityService _cityService)
        {
            cityService = _cityService;
        }

        public async Task<IViewComponentResult> InvokeAsync(int take = 12)
        {
            var cities = await cityService.AllCitiesAsync();

            return View(cities.Take(take).ToList());
        }
    }
}

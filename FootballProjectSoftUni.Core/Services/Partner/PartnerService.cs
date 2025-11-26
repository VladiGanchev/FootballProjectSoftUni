using FootballProjectSoftUni.Core.Contracts.Partner;
using FootballProjectSoftUni.Core.Models.City;
using FootballProjectSoftUni.Core.Models.Partner;
using FootballProjectSoftUni.Infrastructure.Data;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Services.Partner
{
    public class PartnerService : IPartnerService
    {

        private readonly ApplicationDbContext data;
        public PartnerService(ApplicationDbContext _data)
        {
            data = _data;
        }

        public async Task AddPartnerAsync(PartnerViewModel model)
        {
            var partner = new FootballProjectSoftUni.Infrastructure.Data.Models.Partner()
            {
                Name = model.Name,
                ImageUrl = model.ImageUrl
            };

            data.Partners.Add(partner);

            await data.SaveChangesAsync();
        }

        public async Task<IEnumerable<PartnerViewModel>> AllPartnersAsync()
        {
            return await data.Partners
               .Select(x => new PartnerViewModel()
               {
                   Id = x.Id,
                   Name = x.Name,
                   ImageUrl = x.ImageUrl
               }).ToListAsync();
        }

        public async Task<bool> DeletePartnerAsync(int id)
        {
            var partner = await data.Partners.FindAsync(id);

            if (partner == null)
            {
                return false;
            }

            data.Partners.Remove(partner);

            await data.SaveChangesAsync();

            return true;
        }

        public async Task<PartnerViewModel> FindPartnerAsync(PartnerViewModel model, int id)
        {
            var partner = await data.Partners.FindAsync(id);

            if (partner == null)
            {
                return null;
            }

            var ViewModel = new PartnerViewModel
            {
                Id = partner.Id,
                Name = partner.Name
            };

            return ViewModel;
        }
    }
}

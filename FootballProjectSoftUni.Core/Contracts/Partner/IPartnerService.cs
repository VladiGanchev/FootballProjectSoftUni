using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FootballProjectSoftUni.Core.Models.Partner;

namespace FootballProjectSoftUni.Core.Contracts.Partner
{
    public interface IPartnerService
    {
        Task AddPartnerAsync(PartnerViewModel model);
        Task<IEnumerable<PartnerViewModel>> AllPartnersAsync();
        Task<bool> DeletePartnerAsync(int id);
        Task<PartnerViewModel> FindPartnerAsync(PartnerViewModel model, int id);

        /// <summary>Gives the partner a new logo.</summary>
        /// <returns>The address of its old logo, or null when there is no such partner.</returns>
        Task<string?> ChangePartnerImageAsync(int id, string imageUrl);
    }
}

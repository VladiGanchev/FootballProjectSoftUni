using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FootballProjectSoftUni.Infrastructure.Data.Constants.DataConstants;


namespace FootballProjectSoftUni.Core.Models.Partner
{
    public class PartnerViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = RequireErrorMessage)]
        public string Name { get; set; } = string.Empty;

        // The picture is uploaded as a file (not typed in); the site fills in its address.
        public string ImageUrl { get; set; } = string.Empty;
    }
}

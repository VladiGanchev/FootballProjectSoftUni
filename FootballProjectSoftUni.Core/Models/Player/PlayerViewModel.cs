using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FootballProjectSoftUni.Infrastructure.Data.Constants.DataConstants;

namespace FootballProjectSoftUni.Core.Models.Player
{
    public class PlayerViewModel
    {
        public string? Name { get; set; } = string.Empty;
    }
}

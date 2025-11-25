using FootballProjectSoftUni.Core.Models.Home;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Contracts.Home
{
    public interface IHomeService
    {
        Task<HomeViewModel> GetHomePageData();
    }
}

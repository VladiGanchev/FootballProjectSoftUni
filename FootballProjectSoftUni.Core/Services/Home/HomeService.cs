using FootballProjectSoftUni.Core.Contracts.Home;
using FootballProjectSoftUni.Core.Models.Home;
using FootballProjectSoftUni.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Core.Services.Home
{
    public class HomeService : IHomeService
    {
        private readonly ApplicationDbContext context;

        public HomeService(ApplicationDbContext _context)
        {
            context = _context;
        }

        public async Task<HomeViewModel> GetHomePageData()
        {
            var stats = await context.AppStats.FindAsync(1);

            int tournaments = stats.TournamentsCreatedTotal;
            int players = stats.PlayersCreatedTotal;
            int teams = stats.TeamsCreatedTotal;

            HomeViewModel viewModel = new HomeViewModel()
            {
                PlayersCount = players,
                TeamsCount = teams,
                TournamentsCount = tournaments,
                YearOfFoundation = "2025"
            };

            return viewModel;
        }
    }
}

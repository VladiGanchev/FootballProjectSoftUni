using FootballProjectSoftUni.Core.Contracts.City;
using FootballProjectSoftUni.Core.Contracts.Coach;
using FootballProjectSoftUni.Core.Contracts.Home;
using FootballProjectSoftUni.Core.Contracts.Notification;
using FootballProjectSoftUni.Core.Contracts.Referee;
using FootballProjectSoftUni.Core.Contracts.Team;
using FootballProjectSoftUni.Core.Contracts.Tournament;
using FootballProjectSoftUni.Core.Services.City;
using FootballProjectSoftUni.Core.Services.Coach;
using FootballProjectSoftUni.Core.Services.Home;
using FootballProjectSoftUni.Core.Services.Notification;
using FootballProjectSoftUni.Core.Services.Referee;
using FootballProjectSoftUni.Core.Services.Team;
using FootballProjectSoftUni.Core.Services.Tournament;
using FootballProjectSoftUni.Infrastructure.Data;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<ICityService, CityService>();
            services.AddScoped<ITournamentService, TournamentService>();
            services.AddScoped<ICoachService, CoachService>();
            services.AddScoped<ITeamService, TeamService>();
            services.AddScoped<IRefereeService, RefereeService>();
            services.AddScoped<IHomeService, HomeService>();
            services.AddScoped<INotificationService, NotificationService>();

            return services;
        }

        public static IServiceCollection AddApplicationDbContext(this IServiceCollection services, IConfiguration config)
        {
            var connectionString = config.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options =>
                 options.UseSqlServer(connectionString));


            services.AddDatabaseDeveloperPageExceptionFilter();

            return services;
        }

        public static IServiceCollection AddApplicationIdentity(this IServiceCollection services, IConfiguration config)
        {
            services
                .AddDefaultIdentity<ApplicationUser>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = false;
                    options.Password.RequireDigit = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            return services;
        }
    }
}

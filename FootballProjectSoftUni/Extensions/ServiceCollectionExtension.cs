using FootballProjectSoftUni.Core.Contracts.City;
using FootballProjectSoftUni.Core.Contracts.Coach;
using FootballProjectSoftUni.Core.Contracts.Email;
using FootballProjectSoftUni.Core.Contracts.Home;
using FootballProjectSoftUni.Core.Contracts.Message;
using FootballProjectSoftUni.Core.Contracts.Notification;
using FootballProjectSoftUni.Core.Contracts.Partner;
using FootballProjectSoftUni.Core.Contracts.Payment;
using FootballProjectSoftUni.Core.Contracts.Profile;
using FootballProjectSoftUni.Core.Contracts.Referee;
using FootballProjectSoftUni.Core.Contracts.Team;
using FootballProjectSoftUni.Core.Contracts.Tournament;
using FootballProjectSoftUni.Core.Contracts.TournamentJoinPayment;
using FootballProjectSoftUni.Core.Contracts.TournamentReminder;
using FootballProjectSoftUni.Core.Services.City;
using FootballProjectSoftUni.Core.Services.Coach;
using FootballProjectSoftUni.Core.Services.Email;
using FootballProjectSoftUni.Core.Services.EmailSender;
using FootballProjectSoftUni.Core.Services.Home;
using FootballProjectSoftUni.Core.Services.Message;
using FootballProjectSoftUni.Core.Services.Notification;
using FootballProjectSoftUni.Core.Services.Partner;
using FootballProjectSoftUni.Core.Services.Payment;
using FootballProjectSoftUni.Core.Services.Profile;
using FootballProjectSoftUni.Core.Services.Referee;
using FootballProjectSoftUni.Core.Services.Team;
using FootballProjectSoftUni.Core.Services.Tournament;
using FootballProjectSoftUni.Core.Services.TournamentJoinPayment;
using FootballProjectSoftUni.Core.Services.TournamentReminder;
using FootballProjectSoftUni.Infrastructure.Data;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
            services.AddScoped<IPartnerService, PartnerService>();
            services.AddScoped<IContactMessageService, ContactMessageService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<ITournamentJoinPaymentService, TournamentJoinPaymentService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IEmailSender, EmailSenderService>();
            services.AddScoped<ITournamentReminderService, TournamentReminderService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IAccountDeletionService, AccountDeletionService>();
            services.AddScoped<ITournamentDeletionService, TournamentDeletionService>();
            services.AddScoped<ITournamentStartService, TournamentStartService>();
            services.AddScoped<FootballProjectSoftUni.Services.TournamentStartJob>();
            services.AddScoped<FootballProjectSoftUni.Services.TournamentDeletionFollowUp>();
            services.AddScoped<FootballProjectSoftUni.Services.TournamentReminderJob>();
            services.AddScoped<IReminderTexts, FootballProjectSoftUni.Services.ReminderTexts>();
            services.AddScoped<IEmailQueue, FootballProjectSoftUni.Services.HangfireEmailQueue>();
            services.AddScoped<FootballProjectSoftUni.Services.EmailJob>();
            services.AddMemoryCache();
            services.AddScoped<FootballProjectSoftUni.Services.ConfirmationEmailSender>();

            // Pictures of cities, tournaments and partners, kept in wwwroot/images/covers.
            services.AddSingleton(sp => new FootballProjectSoftUni.Core.Services.Gallery.CoverPictureStore(
                sp.GetRequiredService<IWebHostEnvironment>().WebRootPath));
            services.AddScoped<FootballProjectSoftUni.Services.CoverPictureUpload>();

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
                    // Sign-in only with a confirmed e-mail: no accounts on somebody else's address, and a mistyped
                    // address is noticed at once (not when the reminder or the refund e-mail never arrives).
                    options.SignIn.RequireConfirmedAccount = true;
                    options.Password.RequireDigit = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = true;
                    // Length protects a password more than the kinds of characters in it.
                    options.Password.RequiredLength = 8;

                    // Brute-force protection: 5 wrong passwords lock the account for 15 minutes.
                    options.Lockout.AllowedForNewUsers = true;
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddErrorDescriber<FootballProjectSoftUni.Extensions.LocalizedIdentityErrorDescriber>();

            // The built-in Identity UI also brings ~25 pages the site does not use (account deletion
            // without the site's rules, e-mail change, 2FA...). Some of them can be abused (unlimited
            // e-mails, checking which e-mails are registered), so they get no route at all (404).
            services.Configure<RazorPagesOptions>(options =>
            {
                options.Conventions.AddAreaFolderRouteModelConvention("Identity", "/Account", model =>
                {
                    if (!UsedIdentityPages.Contains(model.ViewEnginePath))
                    {
                        model.Selectors.Clear();
                    }
                });
            });

            return services;
        }

        private static readonly HashSet<string> UsedIdentityPages = new(StringComparer.OrdinalIgnoreCase)
        {
            "/Account/Login",
            "/Account/Logout",
            "/Account/Register",
            "/Account/ForgotPassword",
            "/Account/ForgotPasswordConfirmation",
            "/Account/ResetPassword",
            "/Account/ResetPasswordConfirmation",
            "/Account/ChangePassword",
            "/Account/DeleteAccount",
            "/Account/ConfirmEmail",     // link in the registration e-mail
            "/Account/RegisterConfirmation",     // "check your e-mail" after registering
            "/Account/ResendEmailConfirmation",  // a new confirmation link (one per account every few minutes)
            "/Account/AccessDenied"      // shown when a normal user opens an admin page
        };
    }
}

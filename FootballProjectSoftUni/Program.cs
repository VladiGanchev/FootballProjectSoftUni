

using FootballProjectSoftUni.Core.Contracts.Email;
using FootballProjectSoftUni.Core.Job;
using FootballProjectSoftUni.Core.Models.Email;
using FootballProjectSoftUni.Core.Models.Settings;
using FootballProjectSoftUni.Core.Services.Email;
using FootballProjectSoftUni.Core.Services.EmailSender;
using FootballProjectSoftUni.Infrastructure.Data;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;

var builder = WebApplication.CreateBuilder(args);
//var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(connectionString));

//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddApplicationDbContext(builder.Configuration);
builder.Services.AddApplicationIdentity(builder.Configuration);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<AutoValidateAntiforgeryTokenAttribute>();
});

builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];
builder.Services.AddHttpContextAccessor();

builder.Services.AddApplicationServices();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.AddTransient<IEmailService, EmailService>();

builder.Services.AddHangfire(config =>
{
    config.UseSqlServerStorage(
        builder.Configuration.GetConnectionString("DefaultConnection")
    );
});

builder.Services.AddHangfireServer();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var recurringJobs = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    recurringJobs.AddOrUpdate<TournamentReminderJob>(
        "tournament-reminders",
        job => job.SendRemindersAsync(),
        "*/30 * * * *"
    );
}

if (app.Environment.IsDevelopment())
{
    app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error/500");
    app.UseStatusCodePagesWithRedirects("Home/Error?statusCode={0}");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire");

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "Tournament Details",
        pattern: "/Tournament/Details/{id}/{information}",
        defaults: new { Controller = "Tournament", Action = "Details"}
        );
    endpoints.MapDefaultControllerRoute();
    app.MapRazorPages();
});

await app.CreateAdminRoleAsync();

app.Run();

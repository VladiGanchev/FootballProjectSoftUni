

using FootballProjectSoftUni;
using FootballProjectSoftUni.Core.Contracts.Email;
using FootballProjectSoftUni.Core.Models.Email;
using FootballProjectSoftUni.Core.Models.Settings;
using FootballProjectSoftUni.Core.Services.Email;
using FootballProjectSoftUni.Core.Services.EmailSender;
using FootballProjectSoftUni.Infrastructure.Data;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System.Globalization;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);
//var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(connectionString));

//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddApplicationDbContext(builder.Configuration);
builder.Services.AddApplicationIdentity(builder.Configuration);
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddSingleton<Microsoft.Extensions.Options.IConfigureOptions<Microsoft.AspNetCore.Mvc.MvcOptions>, FootballProjectSoftUni.Extensions.LocalizedModelBindingMessages>();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<AutoValidateAntiforgeryTokenAttribute>();

    // Lets the error page explain an expired form instead of showing a generic error.
    options.Filters.Add<FootballProjectSoftUni.Extensions.AntiforgeryFailureMarker>();

    // Amounts: accept "250.5" (number fields) and "250,5" (typed in Bulgarian) in both languages.
    options.ModelBinderProviders.Insert(0, new FootballProjectSoftUni.Extensions.FlexibleDecimalModelBinderProvider());
})
.AddViewLocalization()
.AddDataAnnotationsLocalization(options =>
{
    options.DataAnnotationLocalizerProvider = (type, factory) =>
        factory.Create(typeof(SharedResource));
});

// On the live site every cookie travels over HTTPS only, as a rule (not just because the request happened to be
// HTTPS), and never with a POST from another site. Local development keeps working on plain http://localhost.
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
    options.Secure = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
});

builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
builder.Services.Configure<SiteSettings>(builder.Configuration.GetSection("Site"));
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

// The database is brought up to date once, when the site starts: before the first request and before the
// background jobs. If a migration fails, the site doesn't start (and the log says why).
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
}

using (var scope = app.Services.CreateScope())
{
    var recurringJobs = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    recurringJobs.AddOrUpdate<FootballProjectSoftUni.Services.TournamentReminderJob>(
        "tournament-reminders",
        job => job.SendRemindersAsync(),
        "*/30 * * * *"
    );

    // Tournaments whose start time has come (too few teams -> cancelled with refunds, otherwise teams
    // without an opponent go through) and the organiser's warning 24 h before a tournament with too few teams.
    recurringJobs.AddOrUpdate<FootballProjectSoftUni.Services.TournamentStartJob>(
        "tournament-start",
        job => job.RunAsync(),
        "*/5 * * * *"
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
    // Re-execute (not redirect): keeps the real status code (404, 401...) and the original URL.
    app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");
    app.UseHsts();
}

// Security headers for every response (pages and static files).
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;

    // The site must not be embedded in other sites (clickjacking on admin buttons such as Refund / Delete).
    headers["X-Frame-Options"] = "DENY";
    // Browsers must not guess file types (an uploaded file can't be run as a script/page).
    headers["X-Content-Type-Options"] = "nosniff";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=()";

    // Scripts: only files of the site and the scripts of our pages, which carry this request's random
    // nonce (ScriptNonceTagHelper). A script injected into a page doesn't know it, so the browser refuses it.
    // Styles and fonts only from the site itself; styles may still be inline (the design uses many style=""
    // attributes). Everything else (plugins, foreign frames...) is blocked, and forms may only post to the site
    // itself and to Stripe Checkout.
    var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
    context.Items[FootballProjectSoftUni.Extensions.ScriptNonceTagHelper.ItemKey] = nonce;

    headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        $"script-src 'self' 'nonce-{nonce}'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "font-src 'self' data:; " +
        "img-src 'self' data: https:; " +
        "connect-src 'self'; " +
        "frame-ancestors 'none'; " +
        "base-uri 'self'; " +
        "object-src 'none'; " +
        "form-action 'self' https://checkout.stripe.com";

    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();

var supportedCultures = new[] { new CultureInfo("bg"), new CultureInfo("en") };

var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("bg")
    .AddSupportedCultures("bg", "en")
    .AddSupportedUICultures("bg", "en");

app.UseRequestLocalization(localizationOptions);

app.UseRouting();

// Limit login / register / password attempts per IP (see AuthRateLimitMiddleware).
// After UseRouting: it recognises the page that will handle the request, whatever the exact URL.
app.UseMiddleware<FootballProjectSoftUni.Extensions.AuthRateLimitMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Hangfire dashboard: administrators only (see HangfireAdminAuthorizationFilter).
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new FootballProjectSoftUni.Extensions.HangfireAdminAuthorizationFilter() }
});

//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapControllerRoute(
//        name: "Tournament Details",
//        pattern: "/Tournament/Details/{id}/{information}",
//        defaults: new { Controller = "Tournament", Action = "Details"}
//        );
//    endpoints.MapDefaultControllerRoute();
//    app.MapRazorPages();
//});

app.MapControllers();

app.MapControllerRoute(
    name: "Tournament Details",
    pattern: "/Tournament/Details/{id}/{information}",
    defaults: new { Controller = "Tournament", Action = "Details" }
);

app.MapDefaultControllerRoute();
app.MapRazorPages();

await app.CreateAdminRoleAsync();

app.Run();

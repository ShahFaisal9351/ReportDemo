using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReportDemo.Data;
using ReportDemo.Models;
using ReportDemo.Services;
using ReportDemo.Services.Interfaces;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Register code pages encoding (fixes 1252 issues in RDLC/PDF)
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

// Add services
builder.Services.AddControllersWithViews();

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.Name = "EduSession";
});

// Register custom services
builder.Services.AddScoped<ReportDemo.Services.IRollNumberService, ReportDemo.Services.RollNumberService>();

// Register promotion service
builder.Services.AddScoped<ReportDemo.Services.PromotionService>();
builder.Services.AddScoped<ReportDemo.Services.Interfaces.IPromotionService, ReportDemo.Services.PromotionServiceV2>();

// Configure PostgreSQL to handle DateTime properly
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Add DbContext (Postgres example)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), 
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null)));

// Add Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.SignIn.RequireConfirmedAccount = false; // no email confirmation
    
    // Force users to re-authenticate frequently
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(20);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Simple cookie configuration - session only
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
    options.SlidingExpiration = false;
    options.Cookie.IsEssential = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

var app = builder.Build();

// Force logout on every app startup - GUARANTEED login page
var startupTime = DateTime.UtcNow;
app.Logger.LogInformation($"Application started at: {startupTime}");

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Simple startup authentication cleaner
var isFirstRequest = true;
app.Use(async (context, next) =>
{
    // Check first request only
    if (isFirstRequest)
    {
        isFirstRequest = false;
        
        // If user appears authenticated on first request, clear it
        if (context.User.Identity!.IsAuthenticated)
        {
            context.Response.Redirect("/Account/ForceLogout");
            return;
        }
    }
    
    // Redirect root to login if not authenticated
    if (context.Request.Path == "/" && !context.User.Identity!.IsAuthenticated)
    {
        context.Response.Redirect("/Account/Login");
        return;
    }
    
    await next();
});

// Account route for unauthenticated users (higher priority)
app.MapControllerRoute(
    name: "account",
    pattern: "Account/{action=Login}/{id?}",
    defaults: new { controller = "Account" });

// Default route - will redirect to login if not authenticated due to [Authorize] on HomeController
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed initial data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    // Seed Sessions
    if (!context.Sessions.Any())
    {
        var sessions = new[]
        {
            new Session { Name = "2023-2024", AcademicYear = "2023-2024", StartDate = new DateTime(2023, 9, 1), EndDate = new DateTime(2024, 6, 30), IsCurrent = false },
            new Session { Name = "2024-2025", AcademicYear = "2024-2025", StartDate = new DateTime(2024, 9, 1), EndDate = new DateTime(2025, 6, 30), IsCurrent = true },
            new Session { Name = "2025-2026", AcademicYear = "2025-2026", StartDate = new DateTime(2025, 9, 1), EndDate = new DateTime(2026, 6, 30), IsCurrent = false }
        };
        context.Sessions.AddRange(sessions);
        context.SaveChanges();
    }
    
    // Seed Sections for existing classes
    if (!context.Sections.Any())
    {
        var classes = context.Classes.ToList();
        var sections = new List<Section>();
        
        foreach (var classItem in classes)
        {
            sections.Add(new Section { Name = "A", ClassId = classItem.Id });
            sections.Add(new Section { Name = "B", ClassId = classItem.Id });
        }
        
        if (sections.Any())
        {
            context.Sections.AddRange(sections);
            context.SaveChanges();
        }
    }
}

app.Run();

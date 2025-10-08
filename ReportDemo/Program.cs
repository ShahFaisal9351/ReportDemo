using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReportDemo.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Register code pages encoding (fixes 1252 issues in RDLC/PDF)
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

// Add services
builder.Services.AddControllersWithViews();

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
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure cookies to **always expire on browser close**
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // cookie lifetime
    options.LoginPath = "/Account/Login"; // redirect login
    options.AccessDeniedPath = "/Account/Login"; // redirect denied
    options.SlidingExpiration = false; // disable persistent cookies
});

var app = builder.Build();

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();

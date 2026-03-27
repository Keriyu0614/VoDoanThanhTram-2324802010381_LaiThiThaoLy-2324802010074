using ASC.Solution.Services;
using ASC.Web.Configuration;
using ASC.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ASC.Web.Services;
using ASC.DataAccess.Interfaces;
using ASC.DataAccess;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// 1. DbContext (DUY NHẤT 1 lần)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Identity (DUY NHẤT 1 lần)
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// 3. Custom DI + Config
builder.Services.AddCongfig(builder.Configuration)
                .AddMyDependencyGroup();

// 4. MVC + Razor
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// 5. Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 6. Exception filter
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
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

// Routing
app.MapControllerRoute(
    name: "areaRoute",
    pattern: "{area:exists}/{controller=Home}/{action=Index}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// 7. Seed data (FIX NULL WARNING)
using (var scope = app.Services.CreateScope())
{
    var storageSeed = scope.ServiceProvider.GetRequiredService<IIdentitySeed>();

    await storageSeed.Seed(
        scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>(),
        scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>(),
        scope.ServiceProvider.GetRequiredService<IOptions<ApplicationSettings>>()
    );
}

using (var scope = app.Services.CreateScope())
{
    var navigationCacheOperations = scope.ServiceProvider.GetRequiredService<INavigationCacheOperations>();
    await navigationCacheOperations.CreateNavigationCacheAsync();
}

app.Run();
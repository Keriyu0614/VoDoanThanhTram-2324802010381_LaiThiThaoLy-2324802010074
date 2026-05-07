using ASC.Solution.Services;
using ASC.Web.Configuration;
using ASC.Web.Data;
using ASC.Web.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ASC.Web.Services;
using ASC.DataAccess.Interfaces;
using ASC.DataAccess;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// 1. DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Google Authentication đã được cấu hình trong DependencyInjection.cs (AddCongfig)
//var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
//var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
//if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
//{
//    builder.Services.AddAuthentication()
//        .AddGoogle(options =>
//        {
//            options.ClientId = googleClientId;
//            options.ClientSecret = googleClientSecret;
//        });
//}

// 3. Custom DI + Config (bao gồm Google Auth + Redis Cache)
builder.Services.AddCongfig(builder.Configuration)
                .AddMyDependencyGroup();

// 4. MVC + Razor
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

// 5. Session
// AddDistributedMemoryCache đã được thay bằng Redis trong AddCongfig
//builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 6. Exception filter
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
var supportedCultures = new[] { new CultureInfo("vi-VN") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("vi-VN"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areaRoute",
    pattern: "{area:exists}/{controller=Home}/{action=Index}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.MapHub<ChatHub>("/chatHub");

// 7. Seed data
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

//using (var scope = app.Services.CreateScope())
//{
//    var masterDataCacheOperations = scope.ServiceProvider.GetRequiredService<IMasterDataCacheOperations>();
//    await masterDataCacheOperations.CreateMasterDataCacheAsync();
//}
using (var scope = app.Services.CreateScope())
{
    try
    {
        var masterDataCacheOperations = scope.ServiceProvider
            .GetRequiredService<IMasterDataCacheOperations>();
        await masterDataCacheOperations.CreateMasterDataCacheAsync();
        Console.WriteLine("✅ MasterDataCache created successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ MasterDataCache error: {ex.Message}");
        Console.WriteLine(ex.StackTrace);
    }
}

app.Run();
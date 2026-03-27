using ASC.DataAccess.Interfaces;
using ASC.DataAccess;
using ASC.Solution.Services;
using ASC.Web.Configuration;
using ASC.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ASC.Web.Services
{
    public static class DependencyInjection
    {
        //Config services
        public static IServiceCollection AddCongfig(this IServiceCollection services, IConfiguration config)
        {

            // Add AddDbContext with connectionString to mirage database
            var connectionString = config.GetConnectionString("DefaultConnection") ??
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            //services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
            //Add Options and get data from appsettings.json with "AppSettings"
            services.AddOptions();// IOption
            services.Configure<ApplicationSettings>(config.GetSection("AppSettings"));// IOption<AppSettings>

            return services;
        }
        //Add services
        //Add service
        public static IServiceCollection AddMyDependencyGroup(this IServiceCollection services)
        {
            // DbContext DI
            services.AddScoped<DbContext, ApplicationDbContext>();

            // Services
            services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddScoped<IIdentitySeed, IdentitySeed>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddDistributedMemoryCache();
            services.AddSingleton<INavigationCacheOperations, NavigationCacheOperations>();
            return services;
        }
    }
}
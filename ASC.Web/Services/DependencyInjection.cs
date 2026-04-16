using ASC.DataAccess.Interfaces;
using ASC.DataAccess;
using ASC.Solution.Services;
using ASC.Web.Configuration;
using ASC.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ASC.Web.Services
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCongfig(this IServiceCollection services, IConfiguration config)
        {
            // Google Authentication được cấu hình trong Program.cs sau AddIdentity
            services.AddOptions();
            services.Configure<ApplicationSettings>(config.GetSection("AppSettings"));
            return services;
        }

        public static IServiceCollection AddMyDependencyGroup(this IServiceCollection services)
        {
            services.AddScoped<DbContext, ApplicationDbContext>();
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
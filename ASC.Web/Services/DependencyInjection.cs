using ASC.DataAccess.Interfaces;
using ASC.DataAccess;
using ASC.Solution.Services;
using ASC.Web.Configuration;
using ASC.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ASC.Business.Interfaces;
using ASC.Business;
using ASC.Web.Areas.Configuration.Models;
using ASC.Web.Areas.ServiceRequests.Models;

namespace ASC.Web.Services
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCongfig(this IServiceCollection services, IConfiguration config)
        {
            services.AddOptions();
            services.Configure<ApplicationSettings>(config.GetSection("AppSettings"));
            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    IConfigurationSection googleAuthNSection = config.GetSection("Authentication:Google");
                    options.ClientId = googleAuthNSection["ClientId"];
                    options.ClientSecret = googleAuthNSection["ClientSecret"];
                });

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = config.GetSection("CacheSettings:CacheConnectionString").Value;
                options.InstanceName = config.GetSection("CacheSettings:CacheInstance").Value;
            });

            return services;
        }

        public static IServiceCollection AddMyDependencyGroup(this IServiceCollection services)
        {
            services.AddScoped<DbContext, ApplicationDbContext>();
            services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddScoped<IIdentitySeed, IdentitySeed>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            //services.AddDistributedMemoryCache();
            services.AddSingleton<INavigationCacheOperations, NavigationCacheOperations>();
            services.AddScoped<IMasterDataOperations, MasterDataOperations>();
            services.AddScoped<IMasterDataCacheOperations, MasterDataCacheOperations>();
            services.AddScoped<IServiceRequestOperations, ServiceRequestOperations>();
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
                cfg.AddProfile<ASC.Web.Areas.ServiceRequests.Models.ServiceRequestMappingProfile>();
            });
            return services;
        }
    }
}
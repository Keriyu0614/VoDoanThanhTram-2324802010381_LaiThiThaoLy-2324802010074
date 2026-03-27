using ASC.Web.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace ASC.Web.Data
{
    public class IdentitySeed : IIdentitySeed
    {
        public async Task Seed(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<ApplicationSettings> options)
        {
            string[] roles = new[] { "Admin", "Engineer", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminEmail = "laithithaoly1315@gmail.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var user = new IdentityUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                    await userManager.AddClaimAsync(user, new Claim("IsActive", "True"));
                }
            }

            var engEmail = "engineer@gmail.com";
            var engUser = await userManager.FindByEmailAsync(engEmail);

            if (engUser == null)
            {
                var user = new IdentityUser
                {
                    UserName = "engineer",
                    Email = engEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "Engineer@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Engineer");
                    await userManager.AddClaimAsync(user, new Claim("IsActive", "True"));
                }
            }

            var userEmail = "user@gmail.com";
            var normalUser = await userManager.FindByEmailAsync(userEmail);

            if (normalUser == null)
            {
                var user = new IdentityUser
                {
                    UserName = "user",
                    Email = userEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, "User@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "User");
                    await userManager.AddClaimAsync(user, new Claim("IsActive", "True"));
                }
            }
        }
    }
}
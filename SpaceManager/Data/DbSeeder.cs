using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SpaceManager.Models;

namespace SpaceManager.Data;

public static class DbSeeder
{
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roleNames = { "Admin", "User" };

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
        var configurartions = serviceProvider.GetRequiredService<IConfiguration>();
        var adminEmail = configurartions["DefaultAdmin:Email"];
        var adminPassword = configurartions["DefaultAdmin:Password"];
        var adminFullName = configurartions["DefaultAdmin:FullName"];
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            var newAdminUser = new ApplicationUser
            {
                Email = adminEmail,
                FullName = adminFullName,
                UserName = adminEmail
            };
            var result = await userManager.CreateAsync(newAdminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(newAdminUser, "Admin");
            }
        }
    }
}

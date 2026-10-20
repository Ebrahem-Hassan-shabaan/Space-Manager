using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpaceManager.Models;

namespace SpaceManager.Data;

public static class DbSeeder
{
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roleNames = { "Admin", "User" , "CompanyAdmin" };

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    public static async Task SeedAdminUserAsync(IServiceProvider serviceProvider)
    {
        var configurations = serviceProvider.GetRequiredService<IConfiguration>();
        var adminEmail = configurations["DefaultAdmin:Email"];
        var adminPassword = configurations["DefaultAdmin:Password"];
        var adminFullName = configurations["DefaultAdmin:FullName"];

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            return;
        }

        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            var newAdminUser = new ApplicationUser
            {
                Email = adminEmail,
                FullName = adminFullName ?? "Admin",
                UserName = adminEmail
            };
            var result = await userManager.CreateAsync(newAdminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(newAdminUser, "Admin");
            }
        }
    }

    public static async Task SeedDataAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();
        var workspaceTypes = new[] { "Desk", "Meeting Room", "Private Office", "Conference Room" };

        foreach (var typeName in workspaceTypes)
        {
            var exists = await context.WorkspaceTypes.AnyAsync(wt => wt.Name == typeName);
            if (!exists)
            {
                context.WorkspaceTypes.Add(new WorkspaceType { Name = typeName });
            }
        }

        await context.SaveChangesAsync();
    }
}

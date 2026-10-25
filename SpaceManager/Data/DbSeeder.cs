using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpaceManager.Models;

namespace SpaceManager.Data;

/// <summary>
/// Provides startup initialization methods to seed essential roles, administrator credentials, and baseline workspace types.
/// </summary>
public static class DbSeeder
{
    /// <summary>
    /// Ensures that foundational application roles (Admin, User, CompanyAdmin) exist in the identity store.
    /// </summary>
    /// <param name="serviceProvider">The application service provider for resolving identity dependencies.</param>
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roleNames = { "Admin", "User", "CompanyAdmin" };

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    /// <summary>
    /// Seeds the initial system administrator user configured in application settings if not already present.
    /// </summary>
    /// <param name="serviceProvider">The application service provider for resolving configuration and user management dependencies.</param>
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

    /// <summary>
    /// Seeds baseline domain lookup data such as default workspace category classifications.
    /// </summary>
    /// <param name="serviceProvider">The application service provider for resolving database context.</param>
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

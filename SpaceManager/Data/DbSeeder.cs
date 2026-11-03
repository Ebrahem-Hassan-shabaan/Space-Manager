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

        // Seed initial membership subscription plans
        if (!await context.SubscriptionPlans.AnyAsync())
        {
            var deskType = await context.WorkspaceTypes.FirstOrDefaultAsync(t => t.Name == "Desk");
            var meetingRoomType = await context.WorkspaceTypes.FirstOrDefaultAsync(t => t.Name == "Meeting Room");

            context.SubscriptionPlans.AddRange(
                new SubscriptionPlan
                {
                    Name = "Hot Desk Unlimited",
                    Description = "Ideal for freelancers and remote nomads. Guaranteed desk access, high-speed fiber internet, and complimentary coffee.",
                    MonthlyPrice = 1200.00m,
                    DurationMonths = 1,
                    MaxBookingsPerMonth = 0, // unlimited
                    WorkspaceTypeId = deskType?.Id,
                    IsActive = true
                },
                new SubscriptionPlan
                {
                    Name = "Meeting Room Pass",
                    Description = "Designed for client presentations and collaborative sprints. Up to 15 conference or meeting room sessions per month.",
                    MonthlyPrice = 2500.00m,
                    DurationMonths = 1,
                    MaxBookingsPerMonth = 15,
                    WorkspaceTypeId = meetingRoomType?.Id,
                    IsActive = true
                },
                new SubscriptionPlan
                {
                    Name = "All-Access Executive",
                    Description = "The ultimate co-working pass. Unlimited access to any desk, meeting space, or private office across the entire platform.",
                    MonthlyPrice = 4500.00m,
                    DurationMonths = 1,
                    MaxBookingsPerMonth = 0, // unlimited all-access
                    WorkspaceTypeId = null,  // All-Access Pass
                    IsActive = true
                }
            );

            await context.SaveChangesAsync();
        }

        // Seed or update initial bookable service add-ons
        var defaultAddOns = new[]
        {
            new ServiceAddOn
            {
                Name = "Presentation Projector & Screen",
                Price = 280.00m,
                IsAvailable = true
            },
            new ServiceAddOn
            {
                Name = "Dual Monitor 4K Setup",
                Price = 180.00m,
                IsAvailable = true
            },
            new ServiceAddOn
            {
                Name = "Unlimited Premium Coffee & Refreshments",
                Price = 120.00m,
                IsAvailable = true
            },
            new ServiceAddOn
            {
                Name = "Conference Speakerphone (Jabra)",
                Price = 150.00m,
                IsAvailable = true
            },
            new ServiceAddOn
            {
                Name = "Whiteboard & Brainstorming Kit",
                Price = 80.00m,
                IsAvailable = true
            },
            new ServiceAddOn
            {
                Name = "High-Speed Dedicated IP & LAN Port",
                Price = 140.00m,
                IsAvailable = true
            }
        };

        foreach (var item in defaultAddOns)
        {
            var existing = await context.ServiceAddOns.FirstOrDefaultAsync(sa => sa.Name == item.Name);
            if (existing == null)
            {
                context.ServiceAddOns.Add(item);
            }
            else
            {
                existing.Price = item.Price;
                existing.IsAvailable = true;
            }
        }

        await context.SaveChangesAsync();
    }
}

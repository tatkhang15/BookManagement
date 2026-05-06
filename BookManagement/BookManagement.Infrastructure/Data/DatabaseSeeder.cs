using BookManagement.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace BookManagement.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider, IConfiguration? configuration = null)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roleNames = { "Admin", "SubAdmin", "User" };

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        var adminEmail = "admin@bookmanagement.com";
        var adminUsername = "admin";
        var adminPassword = configuration?["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            adminPassword = "123";
        }

        var adminUser = await userManager.FindByNameAsync(adminUsername)
            ?? await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var newAdmin = new ApplicationUser
            {
                UserName = adminUsername,
                Email = adminEmail,
                FullName = "Administrator",
                Balance = 0m,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(newAdmin);

            if (result.Succeeded)
            {
                adminUser = newAdmin;
            }
        }

        if (adminUser != null)
        {
            adminUser.UserName = adminUsername;
            adminUser.Email = adminEmail;
            adminUser.FullName = string.IsNullOrWhiteSpace(adminUser.FullName) ? "Administrator" : adminUser.FullName;
            adminUser.EmailConfirmed = true;
            adminUser.Status = "Active";
            adminUser.PasswordHash = userManager.PasswordHasher.HashPassword(adminUser, adminPassword);
            await userManager.UpdateAsync(adminUser);

            var roles = await userManager.GetRolesAsync(adminUser);
            if (!roles.Contains("Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}

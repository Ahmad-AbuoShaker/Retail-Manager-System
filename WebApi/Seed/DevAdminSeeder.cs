using Application.Interfaces;
using Application.Interfaces.Data;
using Application.Interfaces.Security;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WebApi.Seed;

public static class DevAdminSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
    {
        var adminEmail = config["Seed:AdminEmail"];
        var adminPassword = config["Seed:AdminPassword"];

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            return;
        }

        var normalizedEmail = adminEmail.Trim().ToLowerInvariant();

        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();

        var existingUser = await context.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
        if (existingUser != null)
        {
            return;
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            PasswordHash = passwordService.HashPassword(adminPassword),
            FirstName = "Admin",
            LastName = "User",
            CreatedDate = DateTime.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var allRoleIds = await context.Roles.Select(r => r.Id).ToListAsync();
        foreach (var roleId in allRoleIds)
        {
            context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleId });
        }
        await context.SaveChangesAsync();
    }
}
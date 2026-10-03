using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PropMate.Api.Enums;
using PropMate.Api.Models;

namespace PropMate.Api.Data;

public static class PropertyManagerSeeder
{
    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        using var scope = services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        var email = configuration["PropertyManager:Email"];
        var password = configuration["PropertyManager:Password"];

        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        email = email.Trim().ToLower();

        var existingUser = await context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (existingUser != null)
        {
            return;
        }

        var user = new User
        {
            FirstName = "Property",
            LastName = "Manager",
            Email = email,
            Role = UserRole.PropertyManager,
            IsActive = true,
            IsVerified = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var passwordHasher = new PasswordHasher<User>();

        user.PasswordHash =
            passwordHasher.HashPassword(user, password);

        context.Users.Add(user);
        await context.SaveChangesAsync();

        Console.WriteLine(
            "Predefined Property Manager account created.");
    }
}
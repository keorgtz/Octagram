using Microsoft.EntityFrameworkCore;
using OctagramDelivery.Shared.Enums;
using OctagramDelivery.Shared.Models;

namespace OctagramDelivery.Api.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();

        if (await context.Users.AnyAsync()) return;

        var admin = new AppUser
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            FullName = "Administrador",
            Rol = UserRole.Admin,
            IsActive = true
        };
        context.Users.Add(admin);
        await context.SaveChangesAsync();
    }
}

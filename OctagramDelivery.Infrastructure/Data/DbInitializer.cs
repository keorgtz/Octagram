using Microsoft.EntityFrameworkCore;
using OctagramDelivery.Domain.Entities;
using OctagramDelivery.Domain.Enums;

namespace OctagramDelivery.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        // Reintenta hasta 12 veces (60 s) por si SQL Server aún no está listo
        for (var attempt = 1; attempt <= 12; attempt++)
        {
            try
            {
                await context.Database.MigrateAsync();
                break;
            }
            catch (Exception ex) when (attempt < 12)
            {
                Console.WriteLine($"[DbInitializer] Intento {attempt}/12 fallido: {ex.Message}. Esperando 5s...");
                await Task.Delay(5_000);
            }
        }

        if (await context.Users.AnyAsync()) return;

        var admin = new AppUser
        {
            Username  = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin"),
            FullName  = "Administrador",
            Rol       = UserRole.Admin,
            IsActive  = true
        };
        context.Users.Add(admin);
        await context.SaveChangesAsync();
    }
}

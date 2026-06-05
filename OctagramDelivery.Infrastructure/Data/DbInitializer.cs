using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OctagramDelivery.Domain.Entities;
using OctagramDelivery.Domain.Enums;

namespace OctagramDelivery.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        for (var attempt = 1; attempt <= 12; attempt++)
        {
            try
            {
                await context.Database.MigrateAsync();
                break;
            }
            catch (SqlException ex) when (IsSchemaConflict(ex))
            {
                // Las tablas ya existen pero el historial de migraciones no tiene el registro.
                // Insertamos el registro manualmente para que EF Core no intente recrearlas.
                Console.WriteLine($"[DbInitializer] Esquema ya existe sin historial. Registrando migraciones pendientes...");
                await MarkPendingMigrationsAsAppliedAsync(context);
                break;
            }
            catch (Exception ex) when (attempt < 12)
            {
                Console.WriteLine($"[DbInitializer] Intento {attempt}/12 — SQL Server no listo: {ex.Message}. Esperando 5s...");
                await Task.Delay(5_000);
            }
        }

        if (await context.Users.AnyAsync()) return;

        context.Users.Add(new AppUser
        {
            Username     = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin"),
            FullName     = "Administrador",
            Rol          = UserRole.Admin,
            IsActive     = true
        });
        await context.SaveChangesAsync();
    }

    private static bool IsSchemaConflict(SqlException ex) =>
        ex.Number == 2714; // "There is already an object named '...' in the database."

    private static async Task MarkPendingMigrationsAsAppliedAsync(AppDbContext context)
    {
        var pending = await context.Database.GetPendingMigrationsAsync();
        foreach (var migration in pending)
        {
            await context.Database.ExecuteSqlRawAsync(
                "INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ({0}, {1})",
                migration, "10.0.8");
            Console.WriteLine($"[DbInitializer] Migración registrada: {migration}");
        }
    }
}

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OctagramDelivery.Domain.Entities;
using OctagramDelivery.Domain.Enums;

namespace OctagramDelivery.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        var migrated = false;

        for (var attempt = 1; attempt <= 12; attempt++)
        {
            try
            {
                await context.Database.MigrateAsync();
                migrated = true;
                break;
            }
            catch (SqlException ex) when (ex.Number == 2714)
            {
                // Las tablas ya existen pero el historial no tiene el registro.
                // Marcamos como aplicadas y luego intentamos el SQL idempotente por si acaso.
                Console.WriteLine("[DbInitializer] Esquema ya existe sin historial. Registrando y re-aplicando migraciones pendientes...");
                await MarkPendingMigrationsAsAppliedAsync(context);
                // Intentar aplicar de todas formas (nuestras migraciones son idempotentes)
                try { await context.Database.MigrateAsync(); } catch { /* ya están marcadas */ }
                migrated = true;
                break;
            }
            catch (Exception ex) when (attempt < 12)
            {
                Console.WriteLine($"[DbInitializer] Intento {attempt}/12 — DB no lista: {ex.Message}. Reintentando en 5s...");
                await Task.Delay(5_000);
            }
            catch (Exception ex)
            {
                // Último intento fallido — loguear pero NO crashear el proceso.
                // El API arranca de todas formas; nginx puede enrutar y Login.razor
                // mostrará las migraciones pendientes para aplicarlas desde la UI.
                Console.Error.WriteLine($"[DbInitializer] ⚠ ERROR FATAL DE MIGRACIÓN después de 12 intentos: {ex}");
                return; // salir sin sembrar, pero sin tirar excepción
            }
        }

        if (!migrated) return;

        try
        {
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
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[DbInitializer] ⚠ Error al sembrar usuario admin: {ex.Message}");
        }
    }

    private static async Task MarkPendingMigrationsAsAppliedAsync(AppDbContext context)
    {
        try
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
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[DbInitializer] No se pudo registrar migraciones: {ex.Message}");
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OctagramDelivery.Api.Data;
using OctagramDelivery.Shared.Models;

namespace OctagramDelivery.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MigrationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public MigrationsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
        // Verificamos si podemos conectar a la base de datos (si existe)
        var canConnect = await _context.Database.CanConnectAsync();

        return Ok(new
        {
            DbExists = canConnect,
            PendingMigrations = pendingMigrations.ToList()
        });
    }

    [HttpPost("apply")]
    public async Task<IActionResult> ApplyMigrations()
    {
        // Aplica migraciones y crea la base de datos si no existe
        await _context.Database.MigrateAsync();

        // Seeding del Admin default
        await SeedDefaultAdminAsync();

        return Ok(new { message = "Migraciones aplicadas correctamente y datos base inicializados." });
    }

    private async Task SeedDefaultAdminAsync()
    {
        // Aseguramos que exista al menos un Tenant
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Name == "Octagram HQ");
        if (tenant == null)
        {
            tenant = new Tenant { Name = "Octagram HQ", Address = "Sede Central" };
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();
        }

        // Verificamos si existe el Admin
        var adminExists = await _context.Users.AnyAsync(u => u.Role == "Admin");
        if (!adminExists)
        {
            var adminUser = new AppUser
            {
                TenantId = tenant.Id,
                Username = "admin",
                PasswordHash = "@Keor0502",
                FullName = "Administrador Global",
                Role = "Admin"
            };
            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OctagramDelivery.Infrastructure.Data;
using OctagramDelivery.Application.DTOs;
using OctagramDelivery.Domain.Enums;
using OctagramDelivery.Domain.Entities;

namespace OctagramDelivery.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NegociosController : ControllerBase
{
    private readonly AppDbContext _ctx;
    public NegociosController(AppDbContext ctx) => _ctx = ctx;

    private UserRole CallerRole => Enum.Parse<UserRole>(User.FindFirst(System.Security.Claims.ClaimTypes.Role)!.Value);
    private List<int> CallerNegocioIds => User.FindAll("negocioId").Select(c => int.Parse(c.Value)).ToList();
    private int CallerId => int.Parse(User.FindFirst("id")!.Value);

    [HttpGet]
    public async Task<ActionResult<List<TenantDto>>> GetAll()
    {
        var rol = CallerRole;
        IQueryable<Tenant> query = _ctx.Tenants.Include(t => t.UsuarioNegocios).Where(t => t.IsActive);

        if (rol != UserRole.Admin)
        {
            var ids = CallerNegocioIds;
            query = query.Where(t => ids.Contains(t.Id));
        }

        var list = await query.ToListAsync();
        return Ok(list.Select(t => new TenantDto
        {
            Id = t.Id,
            Nombre = t.Nombre,
            Direccion = t.Direccion,
            LogoUrl = t.LogoUrl,
            IsActive = t.IsActive,
            TotalRepartidores = t.UsuarioNegocios.Count
        }));
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<TenantDto>> Create([FromBody] CreateTenantRequest req)
    {
        var tenant = new Tenant { Nombre = req.Nombre, Direccion = req.Direccion, LogoUrl = req.LogoUrl };
        _ctx.Tenants.Add(tenant);
        await _ctx.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = tenant.Id }, new TenantDto
        {
            Id = tenant.Id, Nombre = tenant.Nombre, Direccion = tenant.Direccion, LogoUrl = tenant.LogoUrl, IsActive = true
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Update(int id, [FromBody] CreateTenantRequest req)
    {
        var t = await _ctx.Tenants.FindAsync(id);
        if (t == null) return NotFound();
        t.Nombre = req.Nombre;
        t.Direccion = req.Direccion;
        t.LogoUrl = req.LogoUrl;
        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Delete(int id)
    {
        var t = await _ctx.Tenants.FindAsync(id);
        if (t == null) return NotFound();
        t.IsActive = false;
        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    // ── Usuarios del negocio ──────────────────────────────────────

    [HttpGet("{id}/usuarios")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<List<UsuarioNegocioDto>>> GetUsuarios(int id)
    {
        var relaciones = await _ctx.UsuarioNegocios
            .Include(un => un.User)
            .Where(un => un.TenantId == id && un.User!.IsActive)
            .OrderBy(un => un.User!.FullName)
            .ToListAsync();

        return Ok(relaciones.Select(un => new UsuarioNegocioDto
        {
            UserId = un.UserId,
            FullName = un.User!.FullName,
            Username = un.User.Username,
            Rol = un.User.Rol,
            IsActive = un.User.IsActive,
            EsPrincipal = un.EsPrincipal
        }));
    }

    [HttpPost("{id}/usuarios")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> AsignarUsuario(int id, [FromBody] AsignarUsuarioRequest req)
    {
        if (!await _ctx.Tenants.AnyAsync(t => t.Id == id)) return NotFound("Negocio no encontrado.");
        if (!await _ctx.Users.AnyAsync(u => u.Id == req.UserId)) return NotFound("Usuario no encontrado.");
        if (await _ctx.UsuarioNegocios.AnyAsync(un => un.UserId == req.UserId && un.TenantId == id))
            return Conflict("El usuario ya está asignado a este negocio.");

        _ctx.UsuarioNegocios.Add(new UsuarioNegocio { UserId = req.UserId, TenantId = id });
        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/usuarios/nuevo")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> CrearYAsignarUsuario(int id, [FromBody] CrearUsuarioNegocioRequest req)
    {
        if (!await _ctx.Tenants.AnyAsync(t => t.Id == id)) return NotFound("Negocio no encontrado.");
        if (await _ctx.Users.AnyAsync(u => u.Username == req.Username))
            return Conflict("El nombre de usuario ya existe.");

        var user = new AppUser
        {
            Username = req.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            FullName = req.FullName,
            Rol = req.Rol
        };
        _ctx.Users.Add(user);
        await _ctx.SaveChangesAsync();

        _ctx.UsuarioNegocios.Add(new UsuarioNegocio { UserId = user.Id, TenantId = id });
        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}/usuarios/{userId}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> RemoverUsuario(int id, int userId)
    {
        var rel = await _ctx.UsuarioNegocios.FirstOrDefaultAsync(un => un.TenantId == id && un.UserId == userId);
        if (rel == null) return NotFound();
        _ctx.UsuarioNegocios.Remove(rel);
        await _ctx.SaveChangesAsync();
        return NoContent();
    }
}

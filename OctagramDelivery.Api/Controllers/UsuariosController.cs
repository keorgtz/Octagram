using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OctagramDelivery.Api.Data;
using OctagramDelivery.Shared.DTOs;
using OctagramDelivery.Shared.Enums;
using OctagramDelivery.Shared.Models;

namespace OctagramDelivery.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly AppDbContext _ctx;
    public UsuariosController(AppDbContext ctx) => _ctx = ctx;

    private UserRole CallerRole => Enum.Parse<UserRole>(User.FindFirst(System.Security.Claims.ClaimTypes.Role)!.Value);
    private List<int> CallerNegocioIds => User.FindAll("negocioId").Select(c => int.Parse(c.Value)).ToList();

    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAll()
    {
        var users = await _ctx.Users
            .Include(u => u.UsuarioNegocios).ThenInclude(un => un.Tenant)
            .Where(u => u.IsActive)
            .ToListAsync();

        if (CallerRole != UserRole.Admin)
        {
            var ids = CallerNegocioIds;
            users = users.Where(u => u.UsuarioNegocios.Any(un => ids.Contains(un.TenantId))).ToList();
        }

        return Ok(users.Select(u => MapUser(u)));
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest req)
    {
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

        foreach (var nid in req.NegocioIds)
            _ctx.UsuarioNegocios.Add(new UsuarioNegocio { UserId = user.Id, TenantId = nid });
        await _ctx.SaveChangesAsync();

        var created = await _ctx.Users.Include(u => u.UsuarioNegocios).ThenInclude(un => un.Tenant).FirstAsync(u => u.Id == user.Id);
        return CreatedAtAction(nameof(GetAll), new { id = user.Id }, MapUser(created));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest req)
    {
        var user = await _ctx.Users.Include(u => u.UsuarioNegocios).FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound();

        user.FullName = req.FullName;
        user.Rol = req.Rol;
        user.IsActive = req.IsActive;

        if (!string.IsNullOrWhiteSpace(req.NewPassword))
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);

        // Sync negocioIds
        var toRemove = user.UsuarioNegocios.Where(un => !req.NegocioIds.Contains(un.TenantId)).ToList();
        _ctx.UsuarioNegocios.RemoveRange(toRemove);
        var existing = user.UsuarioNegocios.Select(un => un.TenantId).ToList();
        foreach (var nid in req.NegocioIds.Where(n => !existing.Contains(n)))
            _ctx.UsuarioNegocios.Add(new UsuarioNegocio { UserId = id, TenantId = nid });

        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _ctx.Users.FindAsync(id);
        if (user == null) return NotFound();
        user.IsActive = false;
        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    private static UserDto MapUser(AppUser u) => new()
    {
        Id = u.Id,
        Username = u.Username,
        FullName = u.FullName,
        Rol = u.Rol,
        IsActive = u.IsActive,
        NegocioIds = u.UsuarioNegocios.Select(un => un.TenantId).ToList(),
        NegocioNombres = u.UsuarioNegocios.Select(un => un.Tenant?.Nombre ?? "").ToList()
    };
}

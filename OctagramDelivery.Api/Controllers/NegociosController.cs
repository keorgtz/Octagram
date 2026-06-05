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
}

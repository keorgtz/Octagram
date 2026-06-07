using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OctagramDelivery.Application.DTOs;
using OctagramDelivery.Domain.Entities;
using OctagramDelivery.Infrastructure.Data;

namespace OctagramDelivery.Api.Controllers;

[ApiController]
[Route("api/negocios/{negocioId}/grupos-producto")]
[Authorize]
public class GruposProductoController : ControllerBase
{
    private readonly AppDbContext _ctx;
    public GruposProductoController(AppDbContext ctx) => _ctx = ctx;

    [HttpGet]
    public async Task<ActionResult<List<GrupoProductoDto>>> GetAll(int negocioId)
    {
        var grupos = await _ctx.GruposProducto
            .Include(g => g.Productos.Where(p => p.IsActive))
            .Where(g => g.TenantId == negocioId && g.IsActive)
            .OrderBy(g => g.Nombre)
            .ToListAsync();

        return Ok(grupos.Select(MapGrupo));
    }

    [HttpPost]
    public async Task<ActionResult<GrupoProductoDto>> Create(int negocioId, [FromBody] CreateGrupoProductoRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Nombre)) return BadRequest("Nombre requerido");
        var g = new GrupoProducto { TenantId = negocioId, Nombre = req.Nombre.Trim() };
        _ctx.GruposProducto.Add(g);
        await _ctx.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { negocioId }, MapGrupo(g));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int negocioId, int id, [FromBody] CreateGrupoProductoRequest req)
    {
        var g = await _ctx.GruposProducto.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == negocioId);
        if (g == null) return NotFound();
        g.Nombre = req.Nombre.Trim();
        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int negocioId, int id)
    {
        var g = await _ctx.GruposProducto.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == negocioId);
        if (g == null) return NotFound();

        // Desasociar productos antes de marcar inactivo
        await _ctx.Products
            .Where(p => p.GrupoProductoId == id)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.GrupoProductoId, (int?)null));

        g.IsActive = false;
        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    // PUT /api/negocios/{negocioId}/grupos-producto/{id}/productos
    [HttpPut("{id}/productos")]
    public async Task<IActionResult> AsignarProductos(int negocioId, int id, [FromBody] AsignarProductosGrupoRequest req)
    {
        var grupo = await _ctx.GruposProducto.FirstOrDefaultAsync(g => g.Id == id && g.TenantId == negocioId);
        if (grupo == null) return NotFound();

        // Quitar productos que ya no están en la lista
        await _ctx.Products
            .Where(p => p.GrupoProductoId == id && !req.ProductoIds.Contains(p.Id))
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.GrupoProductoId, (int?)null));

        // Asignar los nuevos
        await _ctx.Products
            .Where(p => p.TenantId == negocioId && req.ProductoIds.Contains(p.Id))
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.GrupoProductoId, id));

        return NoContent();
    }

    private static GrupoProductoDto MapGrupo(GrupoProducto g) => new()
    {
        Id = g.Id,
        Nombre = g.Nombre,
        IsActive = g.IsActive,
        ProductoCount = g.Productos.Count,
        ProductoIds = g.Productos.Select(p => p.Id).ToList()
    };
}

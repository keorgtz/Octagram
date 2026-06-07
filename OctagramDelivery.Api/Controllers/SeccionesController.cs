using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OctagramDelivery.Application.DTOs;
using OctagramDelivery.Domain.Entities;
using OctagramDelivery.Infrastructure.Data;

namespace OctagramDelivery.Api.Controllers;

[ApiController]
[Route("api/negocios/{negocioId}/secciones")]
[Authorize]
public class SeccionesController : ControllerBase
{
    private readonly AppDbContext _ctx;
    public SeccionesController(AppDbContext ctx) => _ctx = ctx;

    [HttpGet]
    public async Task<ActionResult<List<SeccionDto>>> GetAll(int negocioId)
    {
        var secciones = await _ctx.Secciones
            .Include(s => s.Clientes.Where(c => c.IsActive))
            .Include(s => s.Stocks).ThenInclude(ss => ss.Producto)
            .Where(s => s.TenantId == negocioId && s.IsActive)
            .OrderBy(s => s.Nombre)
            .ToListAsync();

        return Ok(secciones.Select(MapSeccion));
    }

    [HttpPost]
    public async Task<ActionResult<SeccionDto>> Create(int negocioId, [FromBody] CreateSeccionRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Nombre)) return BadRequest("Nombre requerido");
        var s = new Seccion { TenantId = negocioId, Nombre = req.Nombre.Trim() };
        _ctx.Secciones.Add(s);
        await _ctx.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { negocioId }, MapSeccion(s));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int negocioId, int id, [FromBody] CreateSeccionRequest req)
    {
        var s = await _ctx.Secciones.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == negocioId);
        if (s == null) return NotFound();
        s.Nombre = req.Nombre.Trim();
        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int negocioId, int id)
    {
        var s = await _ctx.Secciones.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == negocioId);
        if (s == null) return NotFound();

        // Desasociar clientes antes de marcar inactiva
        await _ctx.Customers
            .Where(c => c.SeccionId == id)
            .ExecuteUpdateAsync(set => set.SetProperty(c => c.SeccionId, (int?)null));

        s.IsActive = false;
        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    // PUT /api/negocios/{negocioId}/secciones/{id}/clientes
    [HttpPut("{id}/clientes")]
    public async Task<IActionResult> AsignarClientes(int negocioId, int id, [FromBody] AsignarClientesSeccionRequest req)
    {
        var sec = await _ctx.Secciones.FirstOrDefaultAsync(s => s.Id == id && s.TenantId == negocioId);
        if (sec == null) return NotFound();

        // Quitar clientes que ya no están en la lista
        await _ctx.Customers
            .Where(c => c.SeccionId == id && !req.ClienteIds.Contains(c.Id))
            .ExecuteUpdateAsync(set => set.SetProperty(c => c.SeccionId, (int?)null));

        // Asignar los nuevos
        await _ctx.Customers
            .Where(c => c.TenantId == negocioId && req.ClienteIds.Contains(c.Id))
            .ExecuteUpdateAsync(set => set.SetProperty(c => c.SeccionId, id));

        return NoContent();
    }

    // PUT /api/negocios/{negocioId}/secciones/{id}/stocks
    [HttpPut("{id}/stocks")]
    public async Task<IActionResult> UpsertStocks(int negocioId, int id, [FromBody] List<UpsertSeccionStockRequest> req)
    {
        var sec = await _ctx.Secciones.FirstOrDefaultAsync(s => s.Id == id && s.TenantId == negocioId);
        if (sec == null) return NotFound();

        foreach (var item in req)
        {
            var stock = await _ctx.SeccionStocks
                .FirstOrDefaultAsync(ss => ss.SeccionId == id && ss.ProductoId == item.ProductoId);
            if (stock == null)
            {
                _ctx.SeccionStocks.Add(new SeccionStock
                {
                    SeccionId = id,
                    ProductoId = item.ProductoId,
                    CantidadStock = item.CantidadStock
                });
            }
            else
            {
                stock.CantidadStock = item.CantidadStock;
            }
        }
        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/negocios/{negocioId}/secciones/{id}/stocks/{productoId}
    [HttpDelete("{id}/stocks/{productoId}")]
    public async Task<IActionResult> DeleteStock(int negocioId, int id, int productoId)
    {
        var stock = await _ctx.SeccionStocks
            .FirstOrDefaultAsync(ss => ss.SeccionId == id && ss.ProductoId == productoId
                                    && ss.Seccion!.TenantId == negocioId);
        if (stock == null) return NotFound();
        _ctx.SeccionStocks.Remove(stock);
        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    private static SeccionDto MapSeccion(Seccion s) => new()
    {
        Id = s.Id,
        Nombre = s.Nombre,
        IsActive = s.IsActive,
        ClienteIds = s.Clientes.Select(c => c.Id).ToList(),
        Stocks = s.Stocks.Select(ss => new SeccionStockDto
        {
            ProductoId = ss.ProductoId,
            ProductoNombre = ss.Producto?.Nombre ?? "",
            CantidadStock = ss.CantidadStock
        }).ToList()
    };
}

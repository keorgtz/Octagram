using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OctagramDelivery.Api.Data;
using OctagramDelivery.Shared.DTOs;
using OctagramDelivery.Shared.Models;

namespace OctagramDelivery.Api.Controllers;

[ApiController]
[Route("api/negocios/{negocioId}/productos")]
[Authorize]
public class ProductosController : ControllerBase
{
    private readonly AppDbContext _ctx;
    public ProductosController(AppDbContext ctx) => _ctx = ctx;

    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetAll(int negocioId)
    {
        var products = await _ctx.Products
            .Where(p => p.TenantId == negocioId && p.IsActive)
            .ToListAsync();

        return Ok(products.Select(p => new ProductDto
        {
            Id = p.Id, TenantId = p.TenantId, Nombre = p.Nombre,
            TipoMedida = p.TipoMedida, PrecioPorUnidad = p.PrecioPorUnidad, IsActive = p.IsActive
        }));
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(int negocioId, [FromBody] CreateProductRequest req)
    {
        var p = new Product
        {
            TenantId = negocioId,
            Nombre = req.Nombre,
            TipoMedida = req.TipoMedida,
            PrecioPorUnidad = req.PrecioPorUnidad
        };
        _ctx.Products.Add(p);
        await _ctx.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { negocioId },
            new ProductDto { Id = p.Id, TenantId = p.TenantId, Nombre = p.Nombre, TipoMedida = p.TipoMedida, PrecioPorUnidad = p.PrecioPorUnidad, IsActive = true });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int negocioId, int id, [FromBody] CreateProductRequest req)
    {
        var p = await _ctx.Products.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == negocioId);
        if (p == null) return NotFound();
        p.Nombre = req.Nombre;
        p.TipoMedida = req.TipoMedida;
        p.PrecioPorUnidad = req.PrecioPorUnidad;
        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int negocioId, int id)
    {
        var p = await _ctx.Products.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == negocioId);
        if (p == null) return NotFound();
        p.IsActive = false;
        await _ctx.SaveChangesAsync();
        return NoContent();
    }
}

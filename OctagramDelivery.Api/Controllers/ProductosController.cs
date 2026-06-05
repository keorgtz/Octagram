using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OctagramDelivery.Infrastructure.Data;
using OctagramDelivery.Application.DTOs;
using OctagramDelivery.Domain.Entities;

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
            .Include(p => p.PriceTiers)
            .Where(p => p.TenantId == negocioId && p.IsActive)
            .OrderBy(p => p.Nombre)
            .ToListAsync();

        return Ok(products.Select(MapProduct));
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
        return CreatedAtAction(nameof(GetAll), new { negocioId }, MapProduct(p));
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

    // ── Perfiles de precio ────────────────────────────────────────

    [HttpGet("{productoId}/perfiles")]
    public async Task<ActionResult<List<PriceTierDto>>> GetPerfiles(int negocioId, int productoId)
    {
        var perfiles = await _ctx.PriceTiers
            .Where(pt => pt.ProductId == productoId && pt.Product!.TenantId == negocioId)
            .OrderBy(pt => pt.Numero)
            .ToListAsync();

        return Ok(perfiles.Select(MapTier));
    }

    [HttpPost("{productoId}/perfiles")]
    public async Task<ActionResult<PriceTierDto>> CreatePerfil(int negocioId, int productoId, [FromBody] UpsertPriceTierRequest req)
    {
        var product = await _ctx.Products.FirstOrDefaultAsync(p => p.Id == productoId && p.TenantId == negocioId);
        if (product == null) return NotFound();

        var tier = new PriceTier { ProductId = productoId, Numero = req.Numero, Etiqueta = req.Etiqueta, Precio = req.Precio };
        _ctx.PriceTiers.Add(tier);
        await _ctx.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPerfiles), new { negocioId, productoId }, MapTier(tier));
    }

    [HttpPut("{productoId}/perfiles/{tierId}")]
    public async Task<IActionResult> UpdatePerfil(int negocioId, int productoId, int tierId, [FromBody] UpsertPriceTierRequest req)
    {
        var tier = await _ctx.PriceTiers
            .FirstOrDefaultAsync(pt => pt.Id == tierId && pt.ProductId == productoId && pt.Product!.TenantId == negocioId);
        if (tier == null) return NotFound();
        tier.Numero = req.Numero;
        tier.Etiqueta = req.Etiqueta;
        tier.Precio = req.Precio;
        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{productoId}/perfiles/{tierId}")]
    public async Task<IActionResult> DeletePerfil(int negocioId, int productoId, int tierId)
    {
        var tier = await _ctx.PriceTiers
            .FirstOrDefaultAsync(pt => pt.Id == tierId && pt.ProductId == productoId && pt.Product!.TenantId == negocioId);
        if (tier == null) return NotFound();
        _ctx.PriceTiers.Remove(tier);
        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    // ── Helpers ───────────────────────────────────────────────────

    private static ProductDto MapProduct(Product p) => new()
    {
        Id = p.Id, TenantId = p.TenantId, Nombre = p.Nombre,
        TipoMedida = p.TipoMedida, PrecioPorUnidad = p.PrecioPorUnidad, IsActive = p.IsActive,
        Perfiles = p.PriceTiers.OrderBy(t => t.Numero).Select(MapTier).ToList()
    };

    private static PriceTierDto MapTier(PriceTier t) => new()
    {
        Id = t.Id, Numero = t.Numero, Etiqueta = t.Etiqueta, Precio = t.Precio
    };
}

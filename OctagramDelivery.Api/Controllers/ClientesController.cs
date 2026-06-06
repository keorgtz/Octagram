using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OctagramDelivery.Infrastructure.Data;
using OctagramDelivery.Application.DTOs;
using OctagramDelivery.Domain.Entities;

namespace OctagramDelivery.Api.Controllers;

[ApiController]
[Route("api/negocios/{negocioId}/clientes")]
[Authorize]
public class ClientesController : ControllerBase
{
    private readonly AppDbContext _ctx;
    public ClientesController(AppDbContext ctx) => _ctx = ctx;

    [HttpGet]
    public async Task<ActionResult<List<CustomerDto>>> GetAll(int negocioId)
    {
        var customers = await _ctx.Customers
            .Include(c => c.CustomerProducts).ThenInclude(cp => cp.Product).ThenInclude(p => p!.PriceTiers)
            .Include(c => c.CustomerProducts).ThenInclude(cp => cp.PriceTier)
            .Where(c => c.TenantId == negocioId && c.IsActive)
            .OrderBy(c => c.Nombre)
            .ToListAsync();

        return Ok(customers.Select(MapCustomer));
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create(int negocioId, [FromBody] CreateCustomerRequest req)
    {
        var c = new Customer
        {
            TenantId = negocioId,
            Nombre = req.Nombre,
            Direccion = req.Direccion,
            Telefono = req.Telefono,
            DiasEntrega = req.DiasEntrega,
            HoraAproximada = req.HoraAproximada,
            Grupo = req.Grupo
        };
        _ctx.Customers.Add(c);
        await _ctx.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { negocioId }, MapCustomer(c));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int negocioId, int id, [FromBody] CreateCustomerRequest req)
    {
        var c = await _ctx.Customers.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == negocioId);
        if (c == null) return NotFound();
        c.Nombre = req.Nombre;
        c.Direccion = req.Direccion;
        c.Telefono = req.Telefono;
        c.DiasEntrega = req.DiasEntrega;
        c.HoraAproximada = req.HoraAproximada;
        c.Grupo = req.Grupo;
        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int negocioId, int id)
    {
        var c = await _ctx.Customers.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == negocioId);
        if (c == null) return NotFound();
        c.IsActive = false;
        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id}/productos")]
    public async Task<IActionResult> AssignProducts(int negocioId, int id, [FromBody] AssignProductsRequest req)
    {
        var existing = await _ctx.CustomerProducts.Where(cp => cp.CustomerId == id).ToListAsync();
        _ctx.CustomerProducts.RemoveRange(existing);

        foreach (var item in req.Productos)
        {
            _ctx.CustomerProducts.Add(new CustomerProduct
            {
                CustomerId = id,
                ProductId = item.ProductId,
                CantidadHabitual = item.CantidadHabitual,
                PriceTierId = item.PriceTierId
            });
        }
        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    private static CustomerDto MapCustomer(Customer c) => new()
    {
        Id = c.Id,
        TenantId = c.TenantId,
        Nombre = c.Nombre,
        Direccion = c.Direccion,
        Telefono = c.Telefono,
        DiasEntrega = c.DiasEntrega,
        HoraAproximada = c.HoraAproximada,
        IsActive = c.IsActive,
        Grupo = c.Grupo,
        Productos = c.CustomerProducts.Select(cp => new CustomerProductDto
        {
            Id               = cp.Id,
            ProductId        = cp.ProductId,
            ProductNombre    = cp.Product?.Nombre ?? "",
            TipoMedida       = cp.Product?.TipoMedida ?? OctagramDelivery.Domain.Enums.TipoMedida.Pieza,
            PrecioBase       = cp.Product?.PrecioPorUnidad ?? 0,
            CantidadHabitual = cp.CantidadHabitual,
            PriceTierId      = cp.PriceTierId,
            PrecioEfectivo   = cp.PriceTier?.Precio ?? cp.Product?.PrecioPorUnidad ?? 0
        }).ToList()
    };
}

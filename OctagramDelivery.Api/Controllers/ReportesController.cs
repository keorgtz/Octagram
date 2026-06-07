using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using OctagramDelivery.Infrastructure.Data;
using OctagramDelivery.Application.DTOs;
using OctagramDelivery.Domain.Enums;

namespace OctagramDelivery.Api.Controllers;

[ApiController]
[Route("api/reportes")]
[Authorize]
public class ReportesController : ControllerBase
{
    private readonly AppDbContext _ctx;
    public ReportesController(AppDbContext ctx) => _ctx = ctx;

    private UserRole CallerRole => Enum.Parse<UserRole>(User.FindFirst(ClaimTypes.Role)!.Value);
    private List<int> CallerNegocioIds => User.FindAll("negocioId").Select(c => int.Parse(c.Value)).ToList();
    private int CallerId => int.Parse(User.FindFirst("id")!.Value);

    [HttpGet("{jornadaId:int}")]
    public async Task<ActionResult<ReporteJornadaDto>> GetReporte(int jornadaId)
    {
        var jornada = await _ctx.DeliveryDays
            .Include(d => d.Driver)
            .Include(d => d.Tenant)
            .Include(d => d.Rounds)
                .ThenInclude(r => r.Details)
                    .ThenInclude(d => d.Customer)
            .Include(d => d.Rounds)
                .ThenInclude(r => r.Details)
                    .ThenInclude(d => d.Product)
            .Include(d => d.DayCustomers)
                .ThenInclude(dc => dc.Customer)
            .FirstOrDefaultAsync(d => d.Id == jornadaId);

        if (jornada == null) return NotFound();

        var rol = CallerRole;
        if (rol == UserRole.Repartidor && jornada.DriverId != CallerId) return Forbid();
        if (rol != UserRole.Admin && rol != UserRole.Repartidor && !CallerNegocioIds.Contains(jornada.TenantId))
            return Forbid();

        var exclusiones = jornada.DayCustomers.ToDictionary(dc => dc.CustomerId);
        var allDetails  = jornada.Rounds.SelectMany(r => r.Details.Select(d => (Ronda: r, Det: d))).ToList();
        var clienteIds  = allDetails.Select(x => x.Det.CustomerId).Distinct().ToList();

        decimal totalBruto = 0, totalDev = 0, totalEfect = 0, totalExcl = 0;
        var clientesReporte = new List<ReporteClienteDto>();

        foreach (var cliId in clienteIds)
        {
            exclusiones.TryGetValue(cliId, out var excInfo);
            var excluido = excInfo?.ExcluidoDeEfectivo ?? false;
            var nombre   = allDetails.First(x => x.Det.CustomerId == cliId).Det.Customer?.Nombre ?? "";
            var detsCli  = allDetails.Where(x => x.Det.CustomerId == cliId).ToList();

            decimal cliBruto = 0, cliDev = 0, cliNeto = 0;
            var productosReporte = new List<ReporteProductoDto>();

            foreach (var prodId in detsCli.Select(x => x.Det.ProductId).Distinct())
            {
                var detsProd   = detsCli.Where(x => x.Det.ProductId == prodId).ToList();
                var precioUnit = detsProd.First().Det.PrecioUnitario;
                var totalEnt   = detsProd.Sum(x => x.Det.CantidadEntregada);
                var totalDevP  = detsProd.Sum(x => x.Det.CantidadDevuelta);
                var subtotal   = (totalEnt - totalDevP) * precioUnit;

                productosReporte.Add(new ReporteProductoDto
                {
                    ProductoId        = prodId,
                    Nombre            = detsProd.First().Det.Product?.Nombre ?? "",
                    TipoMedida        = detsProd.First().Det.Product?.TipoMedida ?? TipoMedida.Pieza,
                    CantidadEntregada = totalEnt,
                    CantidadDevuelta  = totalDevP,
                    PrecioUnitario    = precioUnit,
                    Subtotal          = subtotal,
                    Vueltas           = detsProd.Select(x => new ReporteVueltaDto
                    {
                        NumeroVuelta = x.Ronda.NumeroRonda,
                        Etiqueta     = x.Ronda.Etiqueta,
                        Entregado    = x.Det.CantidadEntregada,
                        Devuelto     = x.Det.CantidadDevuelta
                    }).ToList()
                });

                cliBruto += totalEnt * precioUnit;
                cliDev   += totalDevP * precioUnit;
                cliNeto  += subtotal;
            }

            totalBruto += cliBruto;
            totalDev   += cliDev;
            if (excluido) totalExcl  += cliNeto;
            else          totalEfect += cliNeto;

            var grupoCliente = allDetails.First(x => x.Det.CustomerId == cliId).Det.Customer?.Grupo;
            clientesReporte.Add(new ReporteClienteDto
            {
                ClienteId          = cliId,
                Nombre             = nombre,
                Grupo              = grupoCliente,
                ExcluidoDeEfectivo = excluido,
                MetodoPago         = excInfo?.MetodoPagoAlternativo ?? MetodoPago.Efectivo,
                TotalBruto         = cliBruto,
                TotalDevuelto      = cliDev,
                TotalNeto          = cliNeto,
                Productos          = productosReporte
            });
        }

        var totalesPorProducto = allDetails
            .GroupBy(x => x.Det.ProductId)
            .Select(g => new ReporteTotalProductoDto
            {
                ProductoId        = g.Key,
                Nombre            = g.First().Det.Product?.Nombre ?? "",
                CantidadEntregada = g.Sum(x => x.Det.CantidadEntregada),
                CantidadDevuelta  = g.Sum(x => x.Det.CantidadDevuelta),
                TotalBruto        = g.Sum(x => x.Det.CantidadEntregada * x.Det.PrecioUnitario),
                TotalNeto         = g.Sum(x => (x.Det.CantidadEntregada - x.Det.CantidadDevuelta) * x.Det.PrecioUnitario)
            }).ToList();

        return Ok(new ReporteJornadaDto
        {
            JornadaId        = jornada.Id,
            TenantId         = jornada.TenantId,
            Fecha            = jornada.Fecha,
            NegocioNombre    = jornada.Tenant?.Nombre ?? "",
            RepartidorNombre = jornada.Driver?.FullName ?? "",
            Estado           = jornada.Estado,
            TotalBruto       = totalBruto,
            TotalDevuelto    = totalDev,
            TotalNeto        = totalBruto - totalDev,
            TotalEfectivo    = totalEfect,
            TotalExcluido    = totalExcl,
            Clientes         = clientesReporte.OrderBy(c => c.Nombre).ToList(),
            TotalesPorProducto = totalesPorProducto
        });
    }
}

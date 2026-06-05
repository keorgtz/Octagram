using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OctagramDelivery.Api.Data;
using OctagramDelivery.Api.Services;
using OctagramDelivery.Shared.DTOs;
using OctagramDelivery.Shared.Enums;

namespace OctagramDelivery.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _ctx;
    private readonly JornadaCalculatorService _calc;

    public DashboardController(AppDbContext ctx, JornadaCalculatorService calc)
    {
        _ctx = ctx;
        _calc = calc;
    }

    private UserRole CallerRole => Enum.Parse<UserRole>(User.FindFirst(System.Security.Claims.ClaimTypes.Role)!.Value);
    private List<int> CallerNegocioIds => User.FindAll("negocioId").Select(c => int.Parse(c.Value)).ToList();

    [HttpGet("supervisor")]
    [Authorize(Roles = $"{nameof(UserRole.Supervisor)},{nameof(UserRole.Gerente)},{nameof(UserRole.Admin)}")]
    public async Task<ActionResult<DashboardNegocioDto>> GetSupervisor([FromQuery] int negocioId, [FromQuery] DateOnly? fecha)
    {
        var f = fecha ?? DateOnly.FromDateTime(DateTime.Today);
        return Ok(await BuildNegocioDto(negocioId, f));
    }

    [HttpGet("gerente")]
    [Authorize(Roles = $"{nameof(UserRole.Gerente)},{nameof(UserRole.Admin)}")]
    public async Task<ActionResult<List<DashboardNegocioDto>>> GetGerente([FromQuery] DateOnly? fecha)
    {
        var f = fecha ?? DateOnly.FromDateTime(DateTime.Today);
        var negocioIds = CallerRole == UserRole.Admin
            ? await _ctx.Tenants.Where(t => t.IsActive).Select(t => t.Id).ToListAsync()
            : CallerNegocioIds;

        var resultado = new List<DashboardNegocioDto>();
        foreach (var nid in negocioIds)
            resultado.Add(await BuildNegocioDto(nid, f));

        return Ok(resultado);
    }

    [HttpGet("admin")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<ActionResult<DashboardAdminDto>> GetAdmin([FromQuery] DateOnly? fecha)
    {
        var f = fecha ?? DateOnly.FromDateTime(DateTime.Today);
        var negocioIds = await _ctx.Tenants.Where(t => t.IsActive).Select(t => t.Id).ToListAsync();

        var negocioList = new List<DashboardNegocioDto>();
        foreach (var nid in negocioIds)
            negocioList.Add(await BuildNegocioDto(nid, f));

        return Ok(new DashboardAdminDto
        {
            TotalNegocios = negocioList.Count,
            TotalRepartidoresActivos = negocioList.Sum(n => n.RepartidoresActivos),
            TotalEfectivoGlobal = negocioList.Sum(n => n.TotalEfectivoEnCalle),
            Negocios = negocioList
        });
    }

    private async Task<DashboardNegocioDto> BuildNegocioDto(int negocioId, DateOnly fecha)
    {
        var jornadas = await _ctx.DeliveryDays
            .Include(d => d.Driver)
            .Include(d => d.Rounds).ThenInclude(r => r.Details).ThenInclude(d => d.Customer)
            .Include(d => d.Rounds).ThenInclude(r => r.Details).ThenInclude(d => d.Product)
            .Include(d => d.DayCustomers)
            .Where(d => d.TenantId == negocioId && d.Fecha == fecha)
            .ToListAsync();

        var tenant = await _ctx.Tenants.FindAsync(negocioId);
        var totalClientes = await _ctx.Customers.CountAsync(c => c.TenantId == negocioId && c.IsActive);
        var repartidores = new List<DashboardRepartidorDto>();

        foreach (var j in jornadas)
        {
            var exclusiones = j.DayCustomers.ToList();
            var totales = _calc.Calcular(j, exclusiones);
            var atendidos = j.Rounds.SelectMany(r => r.Details).Select(d => d.CustomerId).Distinct().Count();

            repartidores.Add(new DashboardRepartidorDto
            {
                UserId = j.DriverId,
                FullName = j.Driver?.FullName ?? "",
                JornadaId = j.Id,
                Estado = j.Estado,
                ClientesAtendidos = atendidos,
                TotalClientes = totalClientes,
                TotalEntregado = totales.TotalEntregadoBruto,
                TotalDevuelto = totales.TotalDevuelto,
                TotalNeto = totales.TotalVentaNeta,
                TotalEfectivo = totales.TotalEfectivo
            });
        }

        return new DashboardNegocioDto
        {
            TenantId = negocioId,
            TenantNombre = tenant?.Nombre ?? "",
            RepartidoresActivos = repartidores.Count(r => r.Estado == JornadaEstado.Abierta),
            RepartidoresCerrados = repartidores.Count(r => r.Estado == JornadaEstado.Cerrada),
            TotalEfectivoEnCalle = repartidores.Sum(r => r.TotalEfectivo),
            TotalNeto = repartidores.Sum(r => r.TotalNeto),
            Repartidores = repartidores
        };
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OctagramDelivery.Infrastructure.Data;
using OctagramDelivery.Api.Hubs;
using OctagramDelivery.Application.Services;
using OctagramDelivery.Application.DTOs;
using OctagramDelivery.Domain.Enums;
using OctagramDelivery.Domain.Entities;

namespace OctagramDelivery.Api.Controllers;

[ApiController]
[Route("api/jornadas")]
[Authorize]
public class JornadasController : ControllerBase
{
    private readonly AppDbContext _ctx;
    private readonly JornadaCalculatorService _calc;
    private readonly IHubContext<JornadaHub> _hub;

    public JornadasController(AppDbContext ctx, JornadaCalculatorService calc, IHubContext<JornadaHub> hub)
    {
        _ctx = ctx;
        _calc = calc;
        _hub = hub;
    }

    private int CallerId => int.Parse(User.FindFirst("id")!.Value);
    private UserRole CallerRole => Enum.Parse<UserRole>(User.FindFirst(System.Security.Claims.ClaimTypes.Role)!.Value);
    private List<int> CallerNegocioIds => User.FindAll("negocioId").Select(c => int.Parse(c.Value)).ToList();

    // GET /api/jornadas/hoy — jornada activa del repartidor
    [HttpGet("hoy")]
    public async Task<ActionResult<JornadaDto>> GetToday([FromQuery] int negocioId)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var driverId = CallerId;

        var jornada = await LoadJornada(negocioId, driverId, hoy);
        if (jornada == null) return NotFound("No hay jornada abierta hoy.");

        return Ok(MapJornada(jornada));
    }

    // GET /api/jornadas/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<JornadaDto>> GetById(int id)
    {
        var jornada = await LoadJornadaById(id);
        if (jornada == null) return NotFound();

        var negocioIds = CallerNegocioIds;
        if (CallerRole != UserRole.Admin && !negocioIds.Contains(jornada.TenantId))
            return Forbid();

        return Ok(MapJornada(jornada));
    }

    // POST /api/jornadas/abrir
    [HttpPost("abrir")]
    public async Task<ActionResult<JornadaDto>> Abrir([FromBody] OpenJornadaRequest req)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var driverId = CallerId;

        var existing = await LoadJornada(req.TenantId, driverId, hoy);
        if (existing != null) return Ok(MapJornada(existing));

        var jornada = new DeliveryDay
        {
            TenantId = req.TenantId,
            DriverId = driverId,
            Fecha = hoy,
            Estado = JornadaEstado.Abierta,
            FechaApertura = DateTime.UtcNow
        };
        _ctx.DeliveryDays.Add(jornada);
        await _ctx.SaveChangesAsync();

        // Primera ronda automática
        var ronda = new DeliveryRound { DeliveryDayId = jornada.Id, NumeroRonda = 1, Etiqueta = "Mañana", Orden = 1 };
        _ctx.DeliveryRounds.Add(ronda);
        await _ctx.SaveChangesAsync();

        var loaded = await LoadJornadaById(jornada.Id);
        return CreatedAtAction(nameof(GetById), new { id = jornada.Id }, MapJornada(loaded!));
    }

    // POST /api/jornadas/{id}/cerrar
    [HttpPost("{id}/cerrar")]
    public async Task<ActionResult<TotalesJornada>> Cerrar(int id)
    {
        var jornada = await LoadJornadaById(id);
        if (jornada == null) return NotFound();
        if (jornada.DriverId != CallerId && CallerRole == UserRole.Repartidor) return Forbid();

        jornada.Estado = JornadaEstado.Cerrada;
        jornada.FechaCierre = DateTime.UtcNow;
        await _ctx.SaveChangesAsync();

        var exclusiones = await _ctx.DeliveryDayCustomers.Where(dc => dc.DeliveryDayId == id).ToListAsync();
        var totales = _calc.Calcular(jornada, exclusiones);

        await _hub.Clients.Group($"negocio-{jornada.TenantId}")
            .SendAsync("JornadaCerrada", new { jornadaId = id, driverId = jornada.DriverId });

        return Ok(totales);
    }

    // POST /api/jornadas/{id}/rondas
    [HttpPost("{id}/rondas")]
    public async Task<ActionResult<RondaDto>> AddRonda(int id, [FromBody] AddRondaRequest req)
    {
        var jornada = await _ctx.DeliveryDays.Include(d => d.Rounds).FirstOrDefaultAsync(d => d.Id == id);
        if (jornada == null) return NotFound();
        if (jornada.Estado != JornadaEstado.Abierta) return BadRequest("La jornada está cerrada.");

        var maxOrden = jornada.Rounds.Any() ? jornada.Rounds.Max(r => r.Orden) : 0;
        var ronda = new DeliveryRound
        {
            DeliveryDayId = id,
            NumeroRonda = jornada.Rounds.Count + 1,
            Etiqueta = string.IsNullOrWhiteSpace(req.Etiqueta) ? $"Vuelta {jornada.Rounds.Count + 1}" : req.Etiqueta,
            Orden = maxOrden + 1
        };
        _ctx.DeliveryRounds.Add(ronda);
        await _ctx.SaveChangesAsync();

        await _hub.Clients.Group($"negocio-{jornada.TenantId}")
            .SendAsync("RondaAgregada", new { jornadaId = id, rondaId = ronda.Id });

        return Ok(new RondaDto { Id = ronda.Id, NumeroRonda = ronda.NumeroRonda, Etiqueta = ronda.Etiqueta, Orden = ronda.Orden });
    }

    // DELETE /api/jornadas/rondas/{rondaId}
    [HttpDelete("rondas/{rondaId}")]
    public async Task<IActionResult> DeleteRonda(int rondaId)
    {
        var ronda = await _ctx.DeliveryRounds.Include(r => r.DeliveryDay).FirstOrDefaultAsync(r => r.Id == rondaId);
        if (ronda == null) return NotFound();

        var totalRondas = await _ctx.DeliveryRounds.CountAsync(r => r.DeliveryDayId == ronda.DeliveryDayId);
        if (totalRondas <= 1) return BadRequest("Debe haber al menos una ronda.");

        _ctx.DeliveryRounds.Remove(ronda);
        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    // PUT /api/jornadas/rondas/{rondaId}/detalles — Bulk upsert
    [HttpPut("rondas/{rondaId}/detalles")]
    public async Task<IActionResult> BulkSaveDetalles(int rondaId, [FromBody] BulkSaveRondaRequest req)
    {
        var ronda = await _ctx.DeliveryRounds.Include(r => r.DeliveryDay).FirstOrDefaultAsync(r => r.Id == rondaId);
        if (ronda == null) return NotFound();
        if (ronda.DeliveryDay?.Estado != JornadaEstado.Abierta) return BadRequest("La jornada está cerrada.");

        var existentes = await _ctx.DeliveryDetails.Where(d => d.DeliveryRoundId == rondaId).ToListAsync();

        foreach (var item in req.Detalles)
        {
            var existente = existentes.FirstOrDefault(e => e.CustomerId == item.ClienteId && e.ProductId == item.ProductoId);
            if (existente != null)
            {
                existente.CantidadEntregada = item.CantidadEntregada;
                existente.CantidadDevuelta = item.CantidadDevuelta;
                existente.PrecioUnitario = item.PrecioUnitario;
                existente.GramajePreset = item.GramajePreset;
            }
            else if (item.CantidadEntregada > 0 || item.CantidadDevuelta > 0)
            {
                _ctx.DeliveryDetails.Add(new DeliveryDetail
                {
                    DeliveryRoundId = rondaId,
                    CustomerId = item.ClienteId,
                    ProductId = item.ProductoId,
                    CantidadEntregada = item.CantidadEntregada,
                    CantidadDevuelta = item.CantidadDevuelta,
                    PrecioUnitario = item.PrecioUnitario,
                    GramajePreset = item.GramajePreset
                });
            }
        }

        await _ctx.SaveChangesAsync();

        await _hub.Clients.Group($"negocio-{ronda.DeliveryDay!.TenantId}")
            .SendAsync("JornadaActualizada", new { jornadaId = ronda.DeliveryDayId });

        return NoContent();
    }

    // PUT /api/jornadas/{id}/clientes/{clienteId}/exclusion
    [HttpPut("{id}/clientes/{clienteId}/exclusion")]
    public async Task<IActionResult> ToggleExclusion(int id, int clienteId, [FromBody] ToggleExclusionRequest req)
    {
        var dc = await _ctx.DeliveryDayCustomers
            .FirstOrDefaultAsync(x => x.DeliveryDayId == id && x.CustomerId == clienteId);

        if (dc == null)
        {
            dc = new DeliveryDayCustomer { DeliveryDayId = id, CustomerId = clienteId };
            _ctx.DeliveryDayCustomers.Add(dc);
        }

        dc.ExcluidoDeEfectivo = req.ExcluidoDeEfectivo;
        dc.MetodoPagoAlternativo = req.MetodoPagoAlternativo;
        dc.NotaExclusion = req.Nota;
        await _ctx.SaveChangesAsync();
        return NoContent();
    }

    // GET /api/jornadas/{id}/totales
    [HttpGet("{id}/totales")]
    public async Task<ActionResult<TotalesJornada>> GetTotales(int id)
    {
        var jornada = await LoadJornadaById(id);
        if (jornada == null) return NotFound();

        var exclusiones = await _ctx.DeliveryDayCustomers.Where(dc => dc.DeliveryDayId == id).ToListAsync();
        return Ok(_calc.Calcular(jornada, exclusiones));
    }

    // ── Helpers ──────────────────────────────────────────────────────
    private async Task<DeliveryDay?> LoadJornada(int tenantId, int driverId, DateOnly fecha)
        => await _ctx.DeliveryDays
            .Include(d => d.Driver)
            .Include(d => d.Tenant)
            .Include(d => d.Rounds).ThenInclude(r => r.Details).ThenInclude(d => d.Customer)
            .Include(d => d.Rounds).ThenInclude(r => r.Details).ThenInclude(d => d.Product)
            .Include(d => d.DayCustomers).ThenInclude(dc => dc.Customer)
            .FirstOrDefaultAsync(d => d.TenantId == tenantId && d.DriverId == driverId && d.Fecha == fecha);

    private async Task<DeliveryDay?> LoadJornadaById(int id)
        => await _ctx.DeliveryDays
            .Include(d => d.Driver)
            .Include(d => d.Tenant)
            .Include(d => d.Rounds).ThenInclude(r => r.Details).ThenInclude(d => d.Customer)
            .Include(d => d.Rounds).ThenInclude(r => r.Details).ThenInclude(d => d.Product)
            .Include(d => d.DayCustomers).ThenInclude(dc => dc.Customer)
            .FirstOrDefaultAsync(d => d.Id == id);

    private static JornadaDto MapJornada(DeliveryDay d) => new()
    {
        Id = d.Id,
        TenantId = d.TenantId,
        TenantNombre = d.Tenant?.Nombre ?? "",
        DriverId = d.DriverId,
        DriverNombre = d.Driver?.FullName ?? "",
        Fecha = d.Fecha,
        Estado = d.Estado,
        FechaApertura = d.FechaApertura,
        FechaCierre = d.FechaCierre,
        Rondas = d.Rounds.OrderBy(r => r.Orden).Select(r => new RondaDto
        {
            Id = r.Id,
            NumeroRonda = r.NumeroRonda,
            Etiqueta = r.Etiqueta,
            Orden = r.Orden,
            Detalles = r.Details.Select(det => new DetalleDto
            {
                Id = det.Id,
                RondaId = det.DeliveryRoundId,
                ClienteId = det.CustomerId,
                ProductoId = det.ProductId,
                CantidadEntregada = det.CantidadEntregada,
                CantidadDevuelta = det.CantidadDevuelta,
                PrecioUnitario = det.PrecioUnitario,
                GramajePreset = det.GramajePreset
            }).ToList()
        }).ToList(),
        Clientes = d.DayCustomers.Select(dc => new ClienteJornadaDto
        {
            ClienteId = dc.CustomerId,
            ClienteNombre = dc.Customer?.Nombre ?? "",
            ExcluidoDeEfectivo = dc.ExcluidoDeEfectivo,
            MetodoPagoAlternativo = dc.MetodoPagoAlternativo,
            NotaExclusion = dc.NotaExclusion
        }).ToList()
    };
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OctagramDelivery.Api.Data;
using OctagramDelivery.Shared.Models;

namespace OctagramDelivery.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requiere JWT
public class DeliveryDaysController : ControllerBase
{
    private readonly AppDbContext _context;

    public DeliveryDaysController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyDeliveries()
    {
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        var tenantId = int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

        var query = _context.DeliveryDays
            .Include(d => d.Rounds)
                .ThenInclude(r => r.Details)
            .Where(d => d.TenantId == tenantId);

        if (role == "Repartidor")
        {
            var driverId = int.Parse(User.FindFirst("Id")?.Value ?? "0");
            query = query.Where(d => d.DriverId == driverId);
        }

        var deliveries = await query.ToListAsync();
        return Ok(deliveries);
    }

    [HttpPost]
    public async Task<IActionResult> StartDeliveryDay([FromBody] DeliveryDay day)
    {
        var tenantId = int.Parse(User.FindFirst("TenantId")?.Value ?? "0");
        var driverId = int.Parse(User.FindFirst("Id")?.Value ?? "0");

        day.TenantId = tenantId;
        
        if(day.DriverId == 0)
            day.DriverId = driverId;

        _context.DeliveryDays.Add(day);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetMyDeliveries), new { id = day.Id }, day);
    }

    [HttpPost("{id}/rounds")]
    public async Task<IActionResult> AddRound(int id)
    {
        var day = await _context.DeliveryDays.Include(d => d.Rounds).FirstOrDefaultAsync(d => d.Id == id);
        if (day == null) return NotFound();

        var newRound = new DeliveryRound
        {
            DeliveryDayId = day.Id,
            RoundNumber = day.Rounds.Count + 1,
            StartedAt = DateTime.UtcNow
        };

        _context.DeliveryRounds.Add(newRound);
        await _context.SaveChangesAsync();

        return Ok(newRound);
    }

    [HttpPut("{id}/recalculate")]
    public async Task<IActionResult> RecalculateTotals(int id)
    {
        var day = await _context.DeliveryDays
            .Include(d => d.Rounds)
            .ThenInclude(r => r.Details)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (day == null) return NotFound();

        decimal totalExpected = 0;
        decimal totalNonCash = 0;

        foreach (var round in day.Rounds)
        {
            foreach (var detail in round.Details)
            {
                var rowTotal = (detail.QuantityDelivered - detail.QuantityReturned) * detail.PriceAtDelivery;
                
                totalExpected += rowTotal;

                if (!detail.IsCashPayment)
                {
                    totalNonCash += rowTotal;
                }
            }
        }

        day.TotalCashExpected = totalExpected - totalNonCash;
        day.TotalNonCash = totalNonCash;
        
        await _context.SaveChangesAsync();

        return Ok(day);
    }
}

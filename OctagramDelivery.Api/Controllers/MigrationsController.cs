using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OctagramDelivery.Infrastructure.Data;

namespace OctagramDelivery.Api.Controllers;

[ApiController]
[Route("api/migrations")]
public class MigrationsController(AppDbContext db) : ControllerBase
{
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var pending = (await db.Database.GetPendingMigrationsAsync()).ToList();
        var applied  = (await db.Database.GetAppliedMigrationsAsync()).ToList();
        return Ok(new { pending, applied = applied.Count });
    }

    [HttpPost("apply")]
    public async Task<IActionResult> Apply()
    {
        await db.Database.MigrateAsync();
        return Ok(new { success = true });
    }
}

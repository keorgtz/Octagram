using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OctagramDelivery.Api.Data;
using OctagramDelivery.Shared.DTOs;
using OctagramDelivery.Shared.Enums;

namespace OctagramDelivery.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.UsuarioNegocios)
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized("Usuario o contraseña incorrectos.");

        var negocioIds = user.UsuarioNegocios.Select(un => un.TenantId).ToList();
        var token = GenerateToken(user, negocioIds);

        return Ok(new LoginResponse
        {
            Token = token,
            UserId = user.Id,
            FullName = user.FullName,
            Rol = user.Rol,
            NegocioIds = negocioIds
        });
    }

    private string GenerateToken(Shared.Models.AppUser user, List<int> negocioIds)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Username),
            new("id", user.Id.ToString()),
            new("fullName", user.FullName),
            new(ClaimTypes.Role, user.Rol.ToString()),
            new("rol", ((int)user.Rol).ToString())
        };

        foreach (var nid in negocioIds)
            claims.Add(new Claim("negocioId", nid.ToString()));

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(10),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

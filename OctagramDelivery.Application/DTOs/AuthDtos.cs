using OctagramDelivery.Domain.Enums;

namespace OctagramDelivery.Application.DTOs;

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public UserRole Rol { get; set; }
    public List<int> NegocioIds { get; set; } = new();
}

using OctagramDelivery.Domain.Enums;

namespace OctagramDelivery.Application.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Rol { get; set; }
    public bool IsActive { get; set; }
    public List<int> NegocioIds { get; set; } = new();
    public List<string> NegocioNombres { get; set; } = new();
}

public class CreateUserRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Rol { get; set; } = UserRole.Repartidor;
    public List<int> NegocioIds { get; set; } = new();
}

public class UpdateUserRequest
{
    public string FullName { get; set; } = string.Empty;
    public UserRole Rol { get; set; }
    public bool IsActive { get; set; }
    public List<int> NegocioIds { get; set; } = new();
    public string? NewPassword { get; set; }
}

public class UsuarioNegocioDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public UserRole Rol { get; set; }
    public bool IsActive { get; set; }
    public bool EsPrincipal { get; set; }
}

public class AsignarUsuarioRequest
{
    public int UserId { get; set; }
}

public class CrearUsuarioNegocioRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Rol { get; set; } = UserRole.Repartidor;
}

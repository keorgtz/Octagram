using OctagramDelivery.Shared.Enums;

namespace OctagramDelivery.Shared.Models;

public class AppUser
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Rol { get; set; } = UserRole.Repartidor;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UsuarioNegocio> UsuarioNegocios { get; set; } = new List<UsuarioNegocio>();
    public ICollection<DeliveryDay> DeliveryDays { get; set; } = new List<DeliveryDay>();
}

using OctagramDelivery.Domain.Enums;

namespace OctagramDelivery.Domain.Entities;

/// <summary>
/// Permisos por rol dentro de un negocio.
/// Define qué permisos tiene cada rol (Supervisor) en cada negocio.
/// Admin y Gerente siempre tienen acceso completo.
/// </summary>
public class NegocioPermiso
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public UserRole Rol { get; set; }
    public int PermisosFlags { get; set; }
}
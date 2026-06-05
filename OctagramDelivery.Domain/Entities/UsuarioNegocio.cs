namespace OctagramDelivery.Domain.Entities;

public class UsuarioNegocio
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public AppUser? User { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public bool EsPrincipal { get; set; }
}

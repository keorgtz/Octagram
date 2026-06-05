namespace OctagramDelivery.Shared.Models;

public class AppUser
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    
    // Roles: Admin, Gerente, Supervisor, Repartidor
    public string Role { get; set; } = "Repartidor";
    
    public bool IsActive { get; set; } = true;
    
    // Si un Gerente puede tener múltiples Tenants, se requeriría una tabla intermedia UserTenants. 
    // Por simplicidad del modelo base de EF Core, si tiene un solo Tenant se asocia directo, 
    // si maneja multiples, usaríamos una relación M2M o dejamos TenantId nulo y validamos por permisos.
    // Vamos a permitir que un gerente se vincule a varios negocios mediante otra tabla si es necesario,
    // o para esta versión base, todos pertenecen a un Tenant principal (salvo Admin).
}

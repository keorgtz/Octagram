namespace OctagramDelivery.Shared.Models;

public class Customer
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    
    // Días de entrega programados (ej. "Lunes, Miércoles, Viernes")
    public string DeliveryDays { get; set; } = string.Empty;
    public TimeSpan? ApproximateDeliveryTime { get; set; }
    
    public bool IsActive { get; set; } = true;
}

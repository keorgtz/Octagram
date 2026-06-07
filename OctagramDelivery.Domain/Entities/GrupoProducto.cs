namespace OctagramDelivery.Domain.Entities;

public class GrupoProducto
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<Product> Productos { get; set; } = new List<Product>();
}

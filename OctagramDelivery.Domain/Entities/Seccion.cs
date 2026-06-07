namespace OctagramDelivery.Domain.Entities;

public class Seccion
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public ICollection<Customer> Clientes { get; set; } = new List<Customer>();
    public ICollection<SeccionStock> Stocks { get; set; } = new List<SeccionStock>();
}

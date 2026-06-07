namespace OctagramDelivery.Domain.Entities;

public class Customer
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    /// <summary>Bitmask: Lun=1, Mar=2, Mie=4, Jue=8, Vie=16, Sab=32, Dom=64</summary>
    public int DiasEntrega { get; set; }
    public TimeSpan? HoraAproximada { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Grupo { get; set; }
    public int? SeccionId { get; set; }
    public Seccion? Seccion { get; set; }
    public ICollection<CustomerProduct> CustomerProducts { get; set; } = new List<CustomerProduct>();
}

namespace OctagramDelivery.Domain.Entities;

public class Tenant
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UsuarioNegocio> UsuarioNegocios { get; set; } = new List<UsuarioNegocio>();
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<DeliveryDay> DeliveryDays { get; set; } = new List<DeliveryDay>();
    public ICollection<GrupoProducto> GruposProducto { get; set; } = new List<GrupoProducto>();
    public ICollection<Seccion> Secciones { get; set; } = new List<Seccion>();
    public ICollection<NegocioPermiso> PermisosPorRol { get; set; } = new List<NegocioPermiso>();
}

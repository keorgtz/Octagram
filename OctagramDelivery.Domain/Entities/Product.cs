using OctagramDelivery.Domain.Enums;

namespace OctagramDelivery.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public TipoMedida TipoMedida { get; set; } = TipoMedida.Pieza;
    public decimal PrecioPorUnidad { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<CustomerProduct> CustomerProducts { get; set; } = new List<CustomerProduct>();
    public ICollection<PriceTier> PriceTiers { get; set; } = new List<PriceTier>();
}

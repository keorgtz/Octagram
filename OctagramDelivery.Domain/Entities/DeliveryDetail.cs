using OctagramDelivery.Domain.Enums;

namespace OctagramDelivery.Domain.Entities;

public class DeliveryDetail
{
    public int Id { get; set; }
    public int DeliveryRoundId { get; set; }
    public DeliveryRound? DeliveryRound { get; set; }
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public decimal CantidadEntregada { get; set; }
    public decimal CantidadDevuelta { get; set; }
    public decimal PrecioUnitario { get; set; }
    public GramajePreset GramajePreset { get; set; } = GramajePreset.Ninguno;
}

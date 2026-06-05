using OctagramDelivery.Domain.Enums;

namespace OctagramDelivery.Domain.Entities;

public class DeliveryDayCustomer
{
    public int Id { get; set; }
    public int DeliveryDayId { get; set; }
    public DeliveryDay? DeliveryDay { get; set; }
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public bool ExcluidoDeEfectivo { get; set; }
    public MetodoPago MetodoPagoAlternativo { get; set; } = MetodoPago.Efectivo;
    public string? NotaExclusion { get; set; }
}

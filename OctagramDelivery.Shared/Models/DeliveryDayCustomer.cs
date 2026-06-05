using OctagramDelivery.Shared.Enums;

namespace OctagramDelivery.Shared.Models;

public class DeliveryDayCustomer
{
    public int Id { get; set; }
    public int DeliveryDayId { get; set; }
    public DeliveryDay? DeliveryDay { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public bool ExcluidoDeEfectivo { get; set; } = false;
    public MetodoPago MetodoPagoAlternativo { get; set; } = MetodoPago.Efectivo;
    public string? NotaExclusion { get; set; }
}

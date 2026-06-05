namespace OctagramDelivery.Domain.Entities;

public class DeliveryRound
{
    public int Id { get; set; }
    public int DeliveryDayId { get; set; }
    public DeliveryDay? DeliveryDay { get; set; }
    public int NumeroRonda { get; set; }
    public string Etiqueta { get; set; } = string.Empty;
    public int Orden { get; set; }
    public ICollection<DeliveryDetail> Details { get; set; } = new List<DeliveryDetail>();
}

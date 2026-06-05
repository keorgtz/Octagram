namespace OctagramDelivery.Shared.Models;

public class DeliveryRound
{
    public int Id { get; set; }
    public int DeliveryDayId { get; set; }
    public DeliveryDay? DeliveryDay { get; set; }
    
    // Ej: 1, 2, 3...
    public int RoundNumber { get; set; }
    
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FinishedAt { get; set; }
    
    public ICollection<DeliveryDetail> Details { get; set; } = new List<DeliveryDetail>();
}

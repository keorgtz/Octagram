namespace OctagramDelivery.Shared.Models;

public class DeliveryDay
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    
    public int DriverId { get; set; }
    public AppUser? Driver { get; set; }
    
    public DateTime Date { get; set; } = DateTime.Today;
    public decimal TotalCashExpected { get; set; }
    public decimal TotalCashCollected { get; set; }
    public decimal TotalNonCash { get; set; }
    
    public ICollection<DeliveryRound> Rounds { get; set; } = new List<DeliveryRound>();
}

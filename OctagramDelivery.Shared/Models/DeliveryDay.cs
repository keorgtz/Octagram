using OctagramDelivery.Shared.Enums;

namespace OctagramDelivery.Shared.Models;

public class DeliveryDay
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    public int DriverId { get; set; }
    public AppUser? Driver { get; set; }

    public DateOnly Fecha { get; set; }
    public JornadaEstado Estado { get; set; } = JornadaEstado.Abierta;
    public DateTime FechaApertura { get; set; } = DateTime.UtcNow;
    public DateTime? FechaCierre { get; set; }

    public ICollection<DeliveryRound> Rounds { get; set; } = new List<DeliveryRound>();
    public ICollection<DeliveryDayCustomer> DayCustomers { get; set; } = new List<DeliveryDayCustomer>();
}

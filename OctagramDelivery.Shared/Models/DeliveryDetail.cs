namespace OctagramDelivery.Shared.Models;

public class DeliveryDetail
{
    public int Id { get; set; }
    public int DeliveryRoundId { get; set; }
    public DeliveryRound? DeliveryRound { get; set; }
    
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    
    // Gramos o Piezas
    public decimal QuantityDelivered { get; set; }
    public decimal QuantityReturned { get; set; }
    
    public decimal PriceAtDelivery { get; set; }
    
    // True si se pagó en efectivo, False si fue transferencia u otro método (se descuenta del gran total)
    public bool IsCashPayment { get; set; } = true;
}

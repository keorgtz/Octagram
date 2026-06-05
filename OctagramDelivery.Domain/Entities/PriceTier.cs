namespace OctagramDelivery.Domain.Entities;

public class PriceTier
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Numero { get; set; }
    public string? Etiqueta { get; set; }
    public decimal Precio { get; set; }
}

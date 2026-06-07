namespace OctagramDelivery.Domain.Entities;

public class SeccionStock
{
    public int SeccionId { get; set; }
    public int ProductoId { get; set; }
    public decimal CantidadStock { get; set; }
    public Seccion? Seccion { get; set; }
    public Product? Producto { get; set; }
}

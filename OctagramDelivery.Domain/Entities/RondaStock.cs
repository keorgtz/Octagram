namespace OctagramDelivery.Domain.Entities;

/// <summary>
/// Stock cargado por vuelta (ronda), sección y producto.
/// Permite que cada vuelta tenga su propia cantidad de stock,
/// diferente del stock base de la sección.
/// </summary>
public class RondaStock
{
    public int RondaId { get; set; }
    public int SeccionId { get; set; }
    public int ProductoId { get; set; }
    public decimal Cantidad { get; set; }
    public DeliveryRound? Ronda { get; set; }
    public Seccion? Seccion { get; set; }
    public Product? Producto { get; set; }
}

using OctagramDelivery.Shared.Enums;

namespace OctagramDelivery.Shared.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public TipoMedida TipoMedida { get; set; }
    public decimal PrecioPorUnidad { get; set; }
    public bool IsActive { get; set; }
}

public class CreateProductRequest
{
    public string Nombre { get; set; } = string.Empty;
    public TipoMedida TipoMedida { get; set; } = TipoMedida.Pieza;
    public decimal PrecioPorUnidad { get; set; }
}

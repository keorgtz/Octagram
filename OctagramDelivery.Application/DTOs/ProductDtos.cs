using OctagramDelivery.Domain.Enums;

namespace OctagramDelivery.Application.DTOs;

public class PriceTierDto
{
    public int Id { get; set; }
    public int Numero { get; set; }
    public string? Etiqueta { get; set; }
    public decimal Precio { get; set; }
}

public class UpsertPriceTierRequest
{
    public int Numero { get; set; }
    public string? Etiqueta { get; set; }
    public decimal Precio { get; set; }
}

public class ProductDto
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public TipoMedida TipoMedida { get; set; }
    public decimal PrecioPorUnidad { get; set; }
    public bool IsActive { get; set; }
    public List<PriceTierDto> Perfiles { get; set; } = new();
}

public class CreateProductRequest
{
    public string Nombre { get; set; } = string.Empty;
    public TipoMedida TipoMedida { get; set; } = TipoMedida.Pieza;
    public decimal PrecioPorUnidad { get; set; }
}

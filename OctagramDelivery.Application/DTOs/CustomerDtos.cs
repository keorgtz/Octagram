using OctagramDelivery.Domain.Enums;

namespace OctagramDelivery.Application.DTOs;

public class CustomerDto
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public int DiasEntrega { get; set; }
    public TimeSpan? HoraAproximada { get; set; }
    public bool IsActive { get; set; }
    public string? Grupo { get; set; }
    public List<CustomerProductDto> Productos { get; set; } = new();
}

public class CustomerProductDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductNombre { get; set; } = string.Empty;
    public TipoMedida TipoMedida { get; set; }
    public decimal CantidadHabitual { get; set; }
    public int? PriceTierId { get; set; }
    public decimal PrecioEfectivo { get; set; }
    public decimal PrecioBase { get; set; }
}

public class CreateCustomerRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public int DiasEntrega { get; set; }
    public TimeSpan? HoraAproximada { get; set; }
    public string? Grupo { get; set; }
}

public class AssignProductsRequest
{
    public List<ProductAssignItem> Productos { get; set; } = new();
}

public class ProductAssignItem
{
    public int ProductId { get; set; }
    public decimal CantidadHabitual { get; set; }
    public int? PriceTierId { get; set; }
}

namespace OctagramDelivery.Shared.DTOs;

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
    public List<CustomerProductDto> Productos { get; set; } = new();
}

public class CustomerProductDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductNombre { get; set; } = string.Empty;
    public decimal CantidadHabitual { get; set; }
    public decimal? PrecioEspecial { get; set; }
}

public class CreateCustomerRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public int DiasEntrega { get; set; }
    public TimeSpan? HoraAproximada { get; set; }
}

public class AssignProductsRequest
{
    public List<CustomerProductItem> Productos { get; set; } = new();
}

public class CustomerProductItem
{
    public int ProductId { get; set; }
    public decimal CantidadHabitual { get; set; }
    public decimal? PrecioEspecial { get; set; }
}

namespace OctagramDelivery.Shared.DTOs;

public class TenantDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; }
    public int TotalRepartidores { get; set; }
}

public class CreateTenantRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? LogoUrl { get; set; }
}

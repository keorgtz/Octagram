namespace OctagramDelivery.Application.DTOs;

public class SeccionDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<int> ClienteIds { get; set; } = new();
    public List<SeccionStockDto> Stocks { get; set; } = new();
}

public class SeccionStockDto
{
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public decimal CantidadStock { get; set; }
}

public class CreateSeccionRequest
{
    public string Nombre { get; set; } = string.Empty;
}

public class UpsertSeccionStockRequest
{
    public int ProductoId { get; set; }
    public decimal CantidadStock { get; set; }
}

public class AsignarClientesSeccionRequest
{
    public List<int> ClienteIds { get; set; } = new();
}

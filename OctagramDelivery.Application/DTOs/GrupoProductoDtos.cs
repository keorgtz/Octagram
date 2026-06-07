namespace OctagramDelivery.Application.DTOs;

public class GrupoProductoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int ProductoCount { get; set; }
    public List<int> ProductoIds { get; set; } = new();
}

public class CreateGrupoProductoRequest
{
    public string Nombre { get; set; } = string.Empty;
}

public class AsignarProductosGrupoRequest
{
    public List<int> ProductoIds { get; set; } = new();
}

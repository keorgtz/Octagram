using OctagramDelivery.Domain.Enums;

namespace OctagramDelivery.Application.DTOs;

public class JornadaDto
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string TenantNombre { get; set; } = string.Empty;
    public int DriverId { get; set; }
    public string DriverNombre { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public JornadaEstado Estado { get; set; }
    public DateTime FechaApertura { get; set; }
    public DateTime? FechaCierre { get; set; }
    public List<RondaDto> Rondas { get; set; } = new();
    public List<ClienteJornadaDto> Clientes { get; set; } = new();
}

public class OpenJornadaRequest { public int TenantId { get; set; } }

public class RondaDto
{
    public int Id { get; set; }
    public int NumeroRonda { get; set; }
    public string Etiqueta { get; set; } = string.Empty;
    public int Orden { get; set; }
    public List<DetalleDto> Detalles { get; set; } = new();
}

public class AddRondaRequest { public string Etiqueta { get; set; } = string.Empty; }

public class DetalleDto
{
    public int Id { get; set; }
    public int RondaId { get; set; }
    public int ClienteId { get; set; }
    public int ProductoId { get; set; }
    public decimal CantidadEntregada { get; set; }
    public decimal CantidadDevuelta { get; set; }
    public decimal PrecioUnitario { get; set; }
    public GramajePreset GramajePreset { get; set; }
}

public class BulkSaveRondaRequest { public List<DetalleUpsertItem> Detalles { get; set; } = new(); }

public class DetalleUpsertItem
{
    public int ClienteId { get; set; }
    public int ProductoId { get; set; }
    public decimal CantidadEntregada { get; set; }
    public decimal CantidadDevuelta { get; set; }
    public decimal PrecioUnitario { get; set; }
    public GramajePreset GramajePreset { get; set; }
}

public class ClienteJornadaDto
{
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public bool ExcluidoDeEfectivo { get; set; }
    public MetodoPago MetodoPagoAlternativo { get; set; }
    public string? NotaExclusion { get; set; }
}

public class ToggleExclusionRequest
{
    public bool ExcluidoDeEfectivo { get; set; }
    public MetodoPago MetodoPagoAlternativo { get; set; }
    public string? Nota { get; set; }
}

public class TotalesJornada
{
    public decimal TotalEntregadoBruto { get; set; }
    public decimal TotalDevuelto { get; set; }
    public decimal TotalVentaNeta { get; set; }
    public decimal TotalEfectivo { get; set; }
    public decimal TotalExcluidoDeEfectivo { get; set; }
    public List<TotalesCliente> PorCliente { get; set; } = new();
    public List<TotalesProducto> PorProducto { get; set; } = new();
}

public class TotalesCliente
{
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public bool ExcluidoDeEfectivo { get; set; }
    public MetodoPago MetodoAlternativo { get; set; }
    public decimal TotalEntregadoBruto { get; set; }
    public decimal TotalDevuelto { get; set; }
    public decimal TotalVentaNeta { get; set; }
    public List<TotalesProductoCliente> PorProducto { get; set; } = new();
    public List<TotalesRonda> PorRonda { get; set; } = new();
}

public class TotalesRonda
{
    public int RondaId { get; set; }
    public string Etiqueta { get; set; } = string.Empty;
    public decimal Entregado { get; set; }
    public decimal Devuelto { get; set; }
    public decimal Neto { get; set; }
}

public class TotalesProducto
{
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public decimal CantidadEntregada { get; set; }
    public decimal CantidadDevuelta { get; set; }
    public decimal TotalBruto { get; set; }
    public decimal TotalNeto { get; set; }
}

public class TotalesProductoCliente
{
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public decimal CantidadEntregada { get; set; }
    public decimal CantidadDevuelta { get; set; }
}

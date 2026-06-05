using OctagramDelivery.Domain.Enums;

namespace OctagramDelivery.Application.DTOs;

public class DashboardRepartidorDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int JornadaId { get; set; }
    public JornadaEstado Estado { get; set; }
    public int ClientesAtendidos { get; set; }
    public int TotalClientes { get; set; }
    public decimal TotalEntregado { get; set; }
    public decimal TotalDevuelto { get; set; }
    public decimal TotalNeto { get; set; }
    public decimal TotalEfectivo { get; set; }
}

public class DashboardNegocioDto
{
    public int TenantId { get; set; }
    public string TenantNombre { get; set; } = string.Empty;
    public int RepartidoresActivos { get; set; }
    public int RepartidoresCerrados { get; set; }
    public decimal TotalEfectivoEnCalle { get; set; }
    public decimal TotalNeto { get; set; }
    public List<DashboardRepartidorDto> Repartidores { get; set; } = new();
}

public class DashboardAdminDto
{
    public int TotalNegocios { get; set; }
    public int TotalRepartidoresActivos { get; set; }
    public decimal TotalEfectivoGlobal { get; set; }
    public List<DashboardNegocioDto> Negocios { get; set; } = new();
}

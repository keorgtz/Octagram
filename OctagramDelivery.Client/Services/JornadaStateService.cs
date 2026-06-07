using OctagramDelivery.Application.DTOs;
using OctagramDelivery.Domain.Enums;

namespace OctagramDelivery.Client.Services;

public class JornadaStateService
{
    public JornadaDto? Jornada { get; private set; }
    public List<ProductDto> Productos { get; private set; } = new();
    public List<CustomerDto> Clientes { get; private set; } = new();
    public Dictionary<(int, int, int), CeldaState> Celdas { get; } = new();
    public Dictionary<int, ClienteJornadaDto> Exclusiones { get; } = new();
    public bool Cargado { get; private set; }
    public bool HayCambiosPendientes { get; set; }

    public event Action? OnChange;

    public void Cargar(JornadaDto jornada, List<ProductDto> productos, List<CustomerDto> clientes)
    {
        Jornada   = jornada;
        Productos = productos;
        Clientes  = clientes;

        Exclusiones.Clear();
        foreach (var c in jornada.Clientes) Exclusiones[c.ClienteId] = c;

        Celdas.Clear();
        foreach (var ronda in jornada.Rondas)
        {
            foreach (var det in ronda.Detalles)
            {
                var key = (ronda.Id, det.ClienteId, det.ProductoId);
                var cp  = clientes.FirstOrDefault(c => c.Id == det.ClienteId)
                                  ?.Productos.FirstOrDefault(p => p.ProductId == det.ProductoId);
                var precio = cp?.PrecioEfectivo
                          ?? productos.FirstOrDefault(p => p.Id == det.ProductoId)?.PrecioPorUnidad
                          ?? det.PrecioUnitario;
                Celdas[key] = new CeldaState
                {
                    Entregado    = det.CantidadEntregada,
                    Devuelto     = det.CantidadDevuelta,
                    Precio       = precio,
                    GramajePreset = det.GramajePreset
                };
            }
        }

        Cargado = true;
        HayCambiosPendientes = false;
        Notificar();
    }

    public void Limpiar()
    {
        Jornada = null;
        Productos.Clear();
        Clientes.Clear();
        Celdas.Clear();
        Exclusiones.Clear();
        Cargado = false;
        Notificar();
    }

    public CeldaState GetOrCreate(int rondaId, int clienteId, int prodId)
    {
        var key = (rondaId, clienteId, prodId);
        if (!Celdas.TryGetValue(key, out var c))
        {
            var cp = Clientes.FirstOrDefault(x => x.Id == clienteId)
                              ?.Productos.FirstOrDefault(p => p.ProductId == prodId);
            var precio = cp?.PrecioEfectivo
                      ?? Productos.FirstOrDefault(p => p.Id == prodId)?.PrecioPorUnidad
                      ?? 0;
            c = new CeldaState { Precio = precio };
            Celdas[key] = c;
        }
        return c;
    }

    public (decimal Bruto, decimal Neto, decimal Efectivo, decimal Devuelto, decimal Excluido) Totales()
    {
        decimal bruto = 0, dev = 0, efect = 0, excl = 0;
        foreach (var cli in Clientes)
        {
            var excluido = Exclusiones.TryGetValue(cli.Id, out var ex) && ex.ExcluidoDeEfectivo;
            decimal neto = 0;
            foreach (var ronda in Jornada?.Rondas ?? new())
                foreach (var prod in Productos)
                {
                    var c = GetOrCreate(ronda.Id, cli.Id, prod.Id);
                    bruto += c.Entregado * c.Precio;
                    dev   += c.Devuelto  * c.Precio;
                    neto  += (c.Entregado - c.Devuelto) * c.Precio;
                }
            if (excluido) excl  += neto;
            else          efect += neto;
        }
        return (bruto, bruto - dev, efect, dev, excl);
    }

    public (decimal Bruto, decimal Neto, decimal Efectivo, decimal Devuelto, decimal Excluido) TotalesFiltrados(IEnumerable<int> clienteIds)
    {
        decimal bruto = 0, dev = 0, efect = 0, excl = 0;
        foreach (var cliId in clienteIds)
        {
            var excluido = Exclusiones.TryGetValue(cliId, out var ex) && ex.ExcluidoDeEfectivo;
            decimal neto = 0;
            foreach (var ronda in Jornada?.Rondas ?? new())
                foreach (var prod in Productos)
                {
                    var c = GetOrCreate(ronda.Id, cliId, prod.Id);
                    bruto += c.Entregado * c.Precio;
                    dev   += c.Devuelto  * c.Precio;
                    neto  += (c.Entregado - c.Devuelto) * c.Precio;
                }
            if (excluido) excl  += neto;
            else          efect += neto;
        }
        return (bruto, bruto - dev, efect, dev, excl);
    }

    public (decimal Neto, decimal Bruto) TotalesCliente(int clienteId)
    {
        decimal bruto = 0, neto = 0;
        foreach (var ronda in Jornada?.Rondas ?? new())
            foreach (var prod in Productos)
            {
                var c = GetOrCreate(ronda.Id, clienteId, prod.Id);
                bruto += c.Entregado * c.Precio;
                neto  += (c.Entregado - c.Devuelto) * c.Precio;
            }
        return (neto, bruto);
    }

    public void Notificar() => OnChange?.Invoke();

    public class CeldaState
    {
        public decimal       Entregado    { get; set; }
        public decimal       Devuelto     { get; set; }
        public decimal       Precio       { get; set; }
        public GramajePreset GramajePreset { get; set; } = GramajePreset.Ninguno;
    }
}

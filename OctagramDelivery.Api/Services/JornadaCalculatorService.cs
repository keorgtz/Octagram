using OctagramDelivery.Shared.DTOs;
using OctagramDelivery.Shared.Enums;
using OctagramDelivery.Shared.Models;

namespace OctagramDelivery.Api.Services;

public class JornadaCalculatorService
{
    public TotalesJornada Calcular(DeliveryDay jornada, List<DeliveryDayCustomer> exclusiones)
    {
        var totales = new TotalesJornada();
        var exclusionMap = exclusiones.ToDictionary(e => e.CustomerId);

        // Agrupar todos los detalles por cliente
        var todoDetalles = jornada.Rounds.SelectMany(r => r.Details).ToList();
        var clienteIds = todoDetalles.Select(d => d.CustomerId).Distinct();

        foreach (var clienteId in clienteIds)
        {
            var excl = exclusionMap.GetValueOrDefault(clienteId);
            var excluido = excl?.ExcluidoDeEfectivo ?? false;
            var metodo = excl?.MetodoPagoAlternativo ?? MetodoPago.Efectivo;
            var nombre = todoDetalles.First(d => d.CustomerId == clienteId).Customer?.Nombre ?? clienteId.ToString();

            var tcli = new TotalesCliente
            {
                ClienteId = clienteId,
                ClienteNombre = nombre,
                ExcluidoDeEfectivo = excluido,
                MetodoAlternativo = metodo
            };

            foreach (var ronda in jornada.Rounds.OrderBy(r => r.Orden))
            {
                var detallesRonda = ronda.Details.Where(d => d.CustomerId == clienteId).ToList();
                var entRonda = detallesRonda.Sum(d => d.CantidadEntregada * d.PrecioUnitario);
                var devRonda = detallesRonda.Sum(d => d.CantidadDevuelta * d.PrecioUnitario);

                tcli.PorRonda.Add(new TotalesRonda
                {
                    RondaId = ronda.Id,
                    Etiqueta = ronda.Etiqueta,
                    Entregado = entRonda,
                    Devuelto = devRonda,
                    Neto = entRonda - devRonda
                });
            }

            // Por producto en este cliente
            var productoIds = todoDetalles.Where(d => d.CustomerId == clienteId).Select(d => d.ProductId).Distinct();
            foreach (var prodId in productoIds)
            {
                var dd = todoDetalles.Where(d => d.CustomerId == clienteId && d.ProductId == prodId);
                tcli.PorProducto.Add(new TotalesProductoCliente
                {
                    ProductoId = prodId,
                    ProductoNombre = dd.First().Product?.Nombre ?? prodId.ToString(),
                    CantidadEntregada = dd.Sum(d => d.CantidadEntregada),
                    CantidadDevuelta = dd.Sum(d => d.CantidadDevuelta)
                });
            }

            tcli.TotalEntregadoBruto = tcli.PorRonda.Sum(r => r.Entregado);
            tcli.TotalDevuelto = tcli.PorRonda.Sum(r => r.Devuelto);
            tcli.TotalVentaNeta = tcli.TotalEntregadoBruto - tcli.TotalDevuelto;

            totales.PorCliente.Add(tcli);

            if (excluido)
                totales.TotalExcluidoDeEfectivo += tcli.TotalVentaNeta;
            else
                totales.TotalEfectivo += tcli.TotalVentaNeta;
        }

        totales.TotalEntregadoBruto = totales.PorCliente.Sum(c => c.TotalEntregadoBruto);
        totales.TotalDevuelto = totales.PorCliente.Sum(c => c.TotalDevuelto);
        totales.TotalVentaNeta = totales.TotalEntregadoBruto - totales.TotalDevuelto;

        // Por producto global
        var todosProductoIds = todoDetalles.Select(d => d.ProductId).Distinct();
        foreach (var prodId in todosProductoIds)
        {
            var dd = todoDetalles.Where(d => d.ProductId == prodId);
            var entregado = dd.Sum(d => d.CantidadEntregada);
            var devuelto = dd.Sum(d => d.CantidadDevuelta);
            var precio = dd.First().PrecioUnitario;
            totales.PorProducto.Add(new TotalesProducto
            {
                ProductoId = prodId,
                ProductoNombre = dd.First().Product?.Nombre ?? prodId.ToString(),
                CantidadEntregada = entregado,
                CantidadDevuelta = devuelto,
                TotalBruto = entregado * precio,
                TotalNeto = (entregado - devuelto) * precio
            });
        }

        return totales;
    }
}

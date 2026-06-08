namespace OctagramDelivery.Domain.Enums;

public enum UserRole    { Admin = 0, Gerente = 1, Supervisor = 2, Repartidor = 3 }
public enum TipoMedida  { Pieza = 0, Gramaje = 1 }
public enum JornadaEstado { Abierta = 0, Cerrada = 1, Revisada = 2 }
public enum GramajePreset { Ninguno = 0, CuartoKg = 1, MedioKg = 2, UnKg = 3, Personalizado = 4 }
public enum MetodoPago  { Efectivo = 0, TarjetaCredito = 1, TarjetaDebito = 2, Transferencia = 3 }

[Flags]
public enum Permiso
{
    Ninguno           = 0,
    VerDashboard      = 1 << 0,   // 1
    VerReportes       = 1 << 1,   // 2
    GestionClientes   = 1 << 2,   // 4
    GestionProductos  = 1 << 3,   // 8
    VerHojaReparto    = 1 << 4,   // 16
    GestionGrupos     = 1 << 5,   // 32
    GestionSecciones  = 1 << 6,   // 64
    AsignarJornada    = 1 << 7,   // 128
    Todo              = VerDashboard | VerReportes | GestionClientes | GestionProductos
                       | VerHojaReparto | GestionGrupos | GestionSecciones | AsignarJornada
}

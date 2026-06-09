using Microsoft.EntityFrameworkCore;
using OctagramDelivery.Domain.Entities;

namespace OctagramDelivery.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<AppUser> Users { get; set; } = null!;
    public DbSet<UsuarioNegocio> UsuarioNegocios { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<PriceTier> PriceTiers { get; set; } = null!;
    public DbSet<CustomerProduct> CustomerProducts { get; set; } = null!;
    public DbSet<DeliveryDay> DeliveryDays { get; set; } = null!;
    public DbSet<DeliveryDayCustomer> DeliveryDayCustomers { get; set; } = null!;
    public DbSet<DeliveryRound> DeliveryRounds { get; set; } = null!;
    public DbSet<DeliveryDetail> DeliveryDetails { get; set; } = null!;
    public DbSet<GrupoProducto> GruposProducto { get; set; } = null!;
    public DbSet<Seccion> Secciones { get; set; } = null!;
    public DbSet<SeccionStock> SeccionStocks { get; set; } = null!;
    public DbSet<NegocioPermiso> NegocioPermisos { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tenant>()
            .HasMany(t => t.Customers).WithOne(c => c.Tenant).HasForeignKey(c => c.TenantId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Tenant>()
            .HasMany(t => t.Products).WithOne(p => p.Tenant).HasForeignKey(p => p.TenantId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Tenant>()
            .HasMany(t => t.DeliveryDays).WithOne(d => d.Tenant).HasForeignKey(d => d.TenantId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UsuarioNegocio>()
            .HasOne(un => un.User).WithMany(u => u.UsuarioNegocios).HasForeignKey(un => un.UserId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<UsuarioNegocio>()
            .HasOne(un => un.Tenant).WithMany(t => t.UsuarioNegocios).HasForeignKey(un => un.TenantId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<UsuarioNegocio>()
            .HasIndex(un => new { un.UserId, un.TenantId }).IsUnique();

        modelBuilder.Entity<PriceTier>()
            .HasOne(pt => pt.Product).WithMany(p => p.PriceTiers).HasForeignKey(pt => pt.ProductId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<PriceTier>()
            .HasIndex(pt => new { pt.ProductId, pt.Numero }).IsUnique();
        modelBuilder.Entity<PriceTier>()
            .Property(pt => pt.Precio).HasPrecision(18, 2);

        modelBuilder.Entity<CustomerProduct>()
            .HasOne(cp => cp.Customer).WithMany(c => c.CustomerProducts).HasForeignKey(cp => cp.CustomerId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<CustomerProduct>()
            .HasOne(cp => cp.Product).WithMany(p => p.CustomerProducts).HasForeignKey(cp => cp.ProductId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<CustomerProduct>()
            .HasOne(cp => cp.PriceTier).WithMany().HasForeignKey(cp => cp.PriceTierId).OnDelete(DeleteBehavior.NoAction).IsRequired(false);
        modelBuilder.Entity<CustomerProduct>()
            .HasIndex(cp => new { cp.CustomerId, cp.ProductId }).IsUnique();

        modelBuilder.Entity<DeliveryDay>()
            .HasOne(d => d.Driver).WithMany(u => u.DeliveryDays).HasForeignKey(d => d.DriverId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<DeliveryDay>()
            .HasMany(d => d.Rounds).WithOne(r => r.DeliveryDay).HasForeignKey(r => r.DeliveryDayId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<DeliveryDay>()
            .HasMany(d => d.DayCustomers).WithOne(dc => dc.DeliveryDay).HasForeignKey(dc => dc.DeliveryDayId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DeliveryDayCustomer>()
            .HasOne(dc => dc.Customer).WithMany().HasForeignKey(dc => dc.CustomerId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<DeliveryDayCustomer>()
            .HasIndex(dc => new { dc.DeliveryDayId, dc.CustomerId }).IsUnique();

        modelBuilder.Entity<DeliveryRound>()
            .HasMany(r => r.Details).WithOne(d => d.DeliveryRound).HasForeignKey(d => d.DeliveryRoundId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DeliveryDetail>()
            .HasOne(d => d.Customer).WithMany().HasForeignKey(d => d.CustomerId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<DeliveryDetail>()
            .HasOne(d => d.Product).WithMany().HasForeignKey(d => d.ProductId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<DeliveryDetail>()
            .HasIndex(d => new { d.DeliveryRoundId, d.CustomerId, d.ProductId }).IsUnique();

        modelBuilder.Entity<CustomerProduct>().Property(p => p.CantidadHabitual).HasPrecision(18, 3);
        modelBuilder.Entity<Product>().Property(p => p.PrecioPorUnidad).HasPrecision(18, 2);
        modelBuilder.Entity<DeliveryDetail>().Property(d => d.CantidadEntregada).HasPrecision(18, 3);
        modelBuilder.Entity<DeliveryDetail>().Property(d => d.CantidadDevuelta).HasPrecision(18, 3);
        modelBuilder.Entity<DeliveryDetail>().Property(d => d.PrecioUnitario).HasPrecision(18, 2);

        // GrupoProducto
        modelBuilder.Entity<Tenant>()
            .HasMany(t => t.GruposProducto).WithOne(g => g.Tenant)
            .HasForeignKey(g => g.TenantId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Product>()
            .HasOne(p => p.GrupoProducto).WithMany(g => g.Productos)
            .HasForeignKey(p => p.GrupoProductoId).OnDelete(DeleteBehavior.NoAction).IsRequired(false);

        // Seccion
        modelBuilder.Entity<Tenant>()
            .HasMany(t => t.Secciones).WithOne(s => s.Tenant)
            .HasForeignKey(s => s.TenantId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Customer>()
            .HasOne(c => c.Seccion).WithMany(s => s.Clientes)
            .HasForeignKey(c => c.SeccionId).OnDelete(DeleteBehavior.NoAction).IsRequired(false);

        // SeccionStock — clave compuesta
        modelBuilder.Entity<SeccionStock>().HasKey(ss => new { ss.SeccionId, ss.ProductoId });
        modelBuilder.Entity<SeccionStock>()
            .HasOne(ss => ss.Seccion).WithMany(s => s.Stocks)
            .HasForeignKey(ss => ss.SeccionId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<SeccionStock>()
            .HasOne(ss => ss.Producto).WithMany(p => p.SeccionStocks)
            .HasForeignKey(ss => ss.ProductoId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<SeccionStock>()
            .Property(ss => ss.CantidadStock).HasPrecision(18, 3);

        // NegocioPermiso — permisos por rol en cada negocio
        modelBuilder.Entity<NegocioPermiso>()
            .HasOne(np => np.Tenant).WithMany(t => t.PermisosPorRol)
            .HasForeignKey(np => np.TenantId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<NegocioPermiso>()
            .HasIndex(np => new { np.TenantId, np.Rol }).IsUnique();
    }
}

using Microsoft.EntityFrameworkCore;
using OctagramDelivery.Shared.Models;

namespace OctagramDelivery.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<AppUser> Users { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<DeliveryDay> DeliveryDays { get; set; } = null!;
    public DbSet<DeliveryRound> DeliveryRounds { get; set; } = null!;
    public DbSet<DeliveryDetail> DeliveryDetails { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Relaciones base
        modelBuilder.Entity<Tenant>()
            .HasMany(t => t.Users)
            .WithOne(u => u.Tenant)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Tenant>()
            .HasMany(t => t.Customers)
            .WithOne(c => c.Tenant)
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Tenant>()
            .HasMany(t => t.Products)
            .WithOne(p => p.Tenant)
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DeliveryDay>()
            .HasMany(d => d.Rounds)
            .WithOne(r => r.DeliveryDay)
            .HasForeignKey(r => r.DeliveryDayId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DeliveryRound>()
            .HasMany(r => r.Details)
            .WithOne(d => d.DeliveryRound)
            .HasForeignKey(d => d.DeliveryRoundId)
            .OnDelete(DeleteBehavior.Cascade);

        // Prevenir borrado en cascada para usuario y cliente dentro del reparto para evitar referencias ciclicas
        modelBuilder.Entity<DeliveryDay>()
            .HasOne(d => d.Driver)
            .WithMany()
            .HasForeignKey(d => d.DriverId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DeliveryDetail>()
            .HasOne(d => d.Customer)
            .WithMany()
            .HasForeignKey(d => d.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DeliveryDetail>()
            .HasOne(d => d.Product)
            .WithMany()
            .HasForeignKey(d => d.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

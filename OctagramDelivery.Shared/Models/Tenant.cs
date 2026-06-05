namespace OctagramDelivery.Shared.Models;

public class Tenant
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

namespace OctagramDelivery.Shared.Models;

public enum UnitType
{
    Piece,
    Weight
}

public class Product
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public UnitType UnitType { get; set; } = UnitType.Piece;
    public decimal Price { get; set; }
    
    public bool IsActive { get; set; } = true;
}

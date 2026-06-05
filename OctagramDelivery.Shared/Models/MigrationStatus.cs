namespace OctagramDelivery.Shared.Models;

public class MigrationStatus
{
    public bool DbExists { get; set; }
    public List<string> PendingMigrations { get; set; } = new List<string>();
}

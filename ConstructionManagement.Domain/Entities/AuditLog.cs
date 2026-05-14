namespace ConstructionManagement.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public AppUser? User { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public string? Metadata { get; set; }
}

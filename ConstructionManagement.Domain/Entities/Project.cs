namespace ConstructionManagement.Domain.Entities;

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public Client? Client { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

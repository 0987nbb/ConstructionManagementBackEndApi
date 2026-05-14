namespace ConstructionManagement.Domain.Entities;

public class Project
{
    public Guid Id { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid ClientId { get; set; }
    public Client? Client { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal Budget { get; set; }
    public decimal SpentAmount { get; set; } = 0m;
    public int ProgressPercentage { get; set; } = 0;
    public string Status { get; set; } = "Planning";
    public Guid? AssignedEngineerId { get; set; }
    public AppUser? AssignedEngineer { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

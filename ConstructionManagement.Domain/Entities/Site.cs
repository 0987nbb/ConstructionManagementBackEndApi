namespace ConstructionManagement.Domain.Entities;

public class Site
{
    public Guid Id { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public Guid? AssignedEngineerId { get; set; }
    public AppUser? AssignedEngineer { get; set; }
    public string Status { get; set; } = "Pending";
    public int ProgressPercentage { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Description { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

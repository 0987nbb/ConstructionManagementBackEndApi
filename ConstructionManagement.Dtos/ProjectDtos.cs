using System.ComponentModel.DataAnnotations;

namespace ConstructionManagement.Dtos;

public static class ProjectStatuses
{
    public const string Planning = "Planning";
    public const string InProgress = "InProgress";
    public const string OnHold = "OnHold";
    public const string Completed = "Completed";
    public const string Cancelled = "Cancelled";

    public static readonly HashSet<string> All = new(StringComparer.OrdinalIgnoreCase)
    {
        Planning,
        InProgress,
        OnHold,
        Completed,
        Cancelled
    };

    public static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return All.FirstOrDefault(x => x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}

public class CreateProjectDto
{
    [Required]
    [StringLength(160, MinimumLength = 2)]
    public string ProjectName { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    public Guid ClientId { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Budget { get; set; }

    [Range(0, 100)]
    public int ProgressPercentage { get; set; } = 0;

    [Required]
    public string Status { get; set; } = ProjectStatuses.Planning;

    public Guid? AssignedEngineerId { get; set; }
}

public class UpdateProjectDto : CreateProjectDto
{
    [Range(0, double.MaxValue)]
    public decimal SpentAmount { get; set; } = 0;
}

public class UpdateProjectStatusDto
{
    [Required]
    public string Status { get; set; } = ProjectStatuses.Planning;
}

public class UpdateProjectProgressDto
{
    [Range(0, 100)]
    public int ProgressPercentage { get; set; }
}

public class ProjectQueryDto
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    public Guid? ClientId { get; set; }
}

public class ProjectDto
{
    public Guid Id { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal Budget { get; set; }
    public decimal SpentAmount { get; set; }
    public int ProgressPercentage { get; set; }
    public string Status { get; set; } = ProjectStatuses.Planning;
    public Guid? AssignedEngineerId { get; set; }
    public string? AssignedEngineerName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ProjectFinancialDto
{
    public Guid Id { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public decimal SpentAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string Status { get; set; } = ProjectStatuses.Planning;
}

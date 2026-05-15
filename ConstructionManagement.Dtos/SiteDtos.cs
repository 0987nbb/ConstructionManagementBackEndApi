using System.ComponentModel.DataAnnotations;

namespace ConstructionManagement.Dtos;

public static class SiteStatuses
{
    public const string Pending = "Pending";
    public const string Active = "Active";
    public const string OnHold = "OnHold";
    public const string Completed = "Completed";

    public static readonly HashSet<string> All = new(StringComparer.OrdinalIgnoreCase)
    {
        Pending,
        Active,
        OnHold,
        Completed
    };

    public static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return All.FirstOrDefault(x => x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}

public class CreateSiteDto
{
    [Required]
    [StringLength(160, MinimumLength = 2)]
    public string SiteName { get; set; } = string.Empty;

    [Required]
    public Guid ProjectId { get; set; }

    [Required]
    [StringLength(500, MinimumLength = 3)]
    public string Location { get; set; } = string.Empty;

    [Range(-90, 90)]
    public decimal? Latitude { get; set; }

    [Range(-180, 180)]
    public decimal? Longitude { get; set; }

    public Guid? AssignedEngineerId { get; set; }

    [Required]
    public string Status { get; set; } = SiteStatuses.Pending;

    [Range(0, 100)]
    public int ProgressPercentage { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }
}

public class UpdateSiteDto : CreateSiteDto
{
}

public class UpdateSiteStatusDto
{
    [Required]
    public string Status { get; set; } = SiteStatuses.Pending;
}

public class UpdateSiteProgressDto
{
    [Range(0, 100)]
    public int ProgressPercentage { get; set; }
}

public class AssignSiteEngineerDto
{
    public Guid? AssignedEngineerId { get; set; }
}

public class SiteQueryDto
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    public Guid? ProjectId { get; set; }
}

public class SiteDto
{
    public Guid Id { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public Guid? AssignedEngineerId { get; set; }
    public string? AssignedEngineerName { get; set; }
    public string Status { get; set; } = SiteStatuses.Pending;
    public int ProgressPercentage { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

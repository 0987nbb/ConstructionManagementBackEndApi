using System.ComponentModel.DataAnnotations;

namespace ConstructionManagement.Dtos;

public class CreateClientDto
{
    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(30)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [StringLength(300, MinimumLength = 5)]
    public string Address { get; set; } = string.Empty;

    [Required]
    [StringLength(160, MinimumLength = 2)]
    public string Company { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

public class UpdateClientDto : CreateClientDto;

public class ClientQueryDto
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
}

public class LinkClientProjectDto
{
    [Required]
    [StringLength(160, MinimumLength = 2)]
    public string ProjectName { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string ProjectCode { get; set; } = string.Empty;
}

public class ProjectLiteDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class ClientDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<ProjectLiteDto> Projects { get; set; } = [];
}

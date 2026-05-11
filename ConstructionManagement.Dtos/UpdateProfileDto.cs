using System.ComponentModel.DataAnnotations;

namespace ConstructionManagement.Dtos;

public class UpdateProfileDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string FullName { get; set; } = string.Empty;

    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }
}

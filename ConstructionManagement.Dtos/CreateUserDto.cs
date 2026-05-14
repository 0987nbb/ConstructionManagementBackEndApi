using System.ComponentModel.DataAnnotations;

namespace ConstructionManagement.Dtos;

public class CreateUserDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Role { get; set; } = string.Empty;

    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    public bool IsActive { get; set; } = true;
}

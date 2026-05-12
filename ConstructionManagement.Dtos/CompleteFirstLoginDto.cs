using System.ComponentModel.DataAnnotations;

namespace ConstructionManagement.Dtos;

public class CompleteFirstLoginDto
{
    [Required]
    [MinLength(1)]
    [StringLength(128)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [StringLength(128)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$",
        ErrorMessage = "Password must contain uppercase, lowercase, number, and special character.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(20, MinimumLength = 5)]
    public string PhoneNumber { get; set; } = string.Empty;
}

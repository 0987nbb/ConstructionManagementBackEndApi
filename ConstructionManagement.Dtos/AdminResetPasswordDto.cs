using System.ComponentModel.DataAnnotations;

namespace ConstructionManagement.Dtos;

public class AdminResetPasswordDto
{
    [Required]
    [MinLength(8)]
    [StringLength(128)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$",
        ErrorMessage = "Password must contain uppercase, lowercase, number, and special character.")]
    public string TemporaryPassword { get; set; } = string.Empty;
}

using System.ComponentModel.DataAnnotations;

namespace ConstructionManagement.Dtos;

public class RefreshTokenRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public class LogoutRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public class RequestPasswordResetDto
{
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordDto
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [StringLength(128)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$",
        ErrorMessage = "Password must contain uppercase, lowercase, number, and special character.")]
    public string NewPassword { get; set; } = string.Empty;
}

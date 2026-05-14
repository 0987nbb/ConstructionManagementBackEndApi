namespace ConstructionManagement.Dtos;

public class AuthResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public string? Role { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public bool IsFirstLogin { get; set; }
}

namespace ConstructionManagement.Dtos;

public class InviteUserResponseDto
{
    public UserDto User { get; set; } = new();
    public string InviteLink { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
}

namespace ConstructionManagement.Dtos;

public class CreateStaffUserResponseDto
{
    public UserDto User { get; set; } = new();
    public DateTime InviteExpiresAtUtc { get; set; }
}

namespace ConstructionManagement.Dtos;

public class CreateStaffUserResponseDto
{
    public UserDto User { get; set; } = new();
    public string TemporaryPassword { get; set; } = string.Empty;
}

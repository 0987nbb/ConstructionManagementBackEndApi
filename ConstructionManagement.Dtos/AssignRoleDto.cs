using System.ComponentModel.DataAnnotations;

namespace ConstructionManagement.Dtos;

public class AssignRoleDto
{
    [Required]
    [StringLength(50)]
    public string Role { get; set; } = string.Empty;
}

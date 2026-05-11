using System.ComponentModel.DataAnnotations;

namespace ConstructionManagement.Dtos;

public class UpdateUserStatusDto
{
    [Required]
    public bool IsActive { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace AuthService.Models.Requests.Roles;

public class CreateRoleRequest
{
    [Required(ErrorMessage = "Role name is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Role name must be between {2} and {1} characters")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Role name can only contain letters, numbers and underscores")]
    public string Name { get; set; } = null!;

    [StringLength(200, ErrorMessage = "Description cannot exceed {1} characters")]
    [DataType(DataType.MultilineText)]
    public string? Description { get; set; }
}
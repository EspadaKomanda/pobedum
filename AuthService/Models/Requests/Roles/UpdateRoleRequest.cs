using System.ComponentModel.DataAnnotations;

namespace AuthService.Models.Requests.Roles;

public class UpdateRoleRequest
{
    [Required(ErrorMessage = "Role ID is required")]
    public Guid Id { get; set; }

    [StringLength(50, MinimumLength = 3, ErrorMessage = "Role name must be between {2} and {1} characters")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Role name can only contain letters, numbers and underscores")]
    public string? Name { get; set; }

    [StringLength(200, ErrorMessage = "Description cannot exceed {1} characters")]
    [DataType(DataType.MultilineText)]
    public string? Description { get; set; }
}

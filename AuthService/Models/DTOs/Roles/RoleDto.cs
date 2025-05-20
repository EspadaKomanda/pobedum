using System.ComponentModel.DataAnnotations;

namespace AuthService.Models.DTOs.Roles;

using System.ComponentModel.DataAnnotations;

public class RoleDto
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Role name is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Role name must be between {2} and {1} characters")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Role name can only contain letters, numbers and underscores")]
    public string Name { get; set; } = null!;

    [StringLength(200, ErrorMessage = "Description cannot exceed {1} characters")]
    [DataType(DataType.MultilineText)]
    public string? Description { get; set; }

    [Required]
    [Display(Name = "Creation Date")]
    public DateTime CreatedAt { get; set; }

    [Display(Name = "Last Updated")]
    public DateTime? UpdatedAt { get; set; }
}

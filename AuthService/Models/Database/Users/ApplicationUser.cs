using System.ComponentModel.DataAnnotations;
using AuthService.Models.Database.Roles;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Models.Database.Users;

public class ApplicationUser : IdentityUser<Guid>
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, ErrorMessage = "Username must be less than 50 characters")]
    public string Username { get; set; } = null!;
    public ApplicationRole Role { get; set; }
}
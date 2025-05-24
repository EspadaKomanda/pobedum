using AuthService.Models.Database.Users;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Models.Database.Roles;


public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() : base() { }
    public ApplicationRole(string roleName) : base(roleName) { }
    public string? Description { get; set; }
    
}

using Microsoft.AspNetCore.Identity;

namespace AuthService.Models.Database;

public class User : IdentityUser
{
    public string Fullname { get; set; } = string.Empty;
}
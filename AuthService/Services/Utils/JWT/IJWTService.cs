using System.IdentityModel.Tokens.Jwt;
using AuthService.Models.Database;
using AuthService.Models.Database.Users;

namespace AuthService.Services.JWT;

public interface IJwtService
{ 
    string GenerateAccessToken(ApplicationUser user);
    JwtSecurityToken ValidateAccessToken(string token);
}
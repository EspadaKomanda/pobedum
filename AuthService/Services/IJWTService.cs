using AuthService.Models.Database;

namespace AuthService.Services;

public interface IJWTService
{
    public Task<string> GenerateToken(User user);
}
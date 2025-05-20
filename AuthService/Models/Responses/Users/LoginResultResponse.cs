namespace AuthService.Models.Responses.Users;

public class LoginResultResponse
{
    public bool Success { get; set; }
    public string? Token { get; set; }
}
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Database.Repositories;
using AuthService.Models.Database.Users;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services.JWT;

public class JwtService : IJwtService
{
    #region Fields

    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtService> _logger;
    private readonly UnitOfWork _unitOfWork;
    
    #endregion
   

    public JwtService(IConfiguration configuration, ILogger<JwtService> logger, UnitOfWork unitOfWork)
    {
        _configuration = configuration;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public string GenerateAccessToken(ApplicationUser user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Sid, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.UserName),
            new Claim(ClaimTypes.Role, user.Role.Name)
        };

        var expires = DateTime.UtcNow.AddDays(double.Parse(jwtSettings["AccessTokenExpirationMinutes"]));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }


    public JwtSecurityToken ValidateAccessToken(string token)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        SecurityToken validatedToken;
        var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
        
        return validatedToken as JwtSecurityToken;
    }
    
}
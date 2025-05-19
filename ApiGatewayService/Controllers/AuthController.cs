using ApiGatewayService.Models.Microservices.AuthService.Requests.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using LoginRequest = ApiGatewayService.Models.Microservices.AuthService.Requests.Users.LoginRequest;

namespace ApiGatewayService.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    #region Services
    
    private readonly ILogger<AuthController> _logger;
    
    #endregion

    #region Constructor

    public AuthController(ILogger<AuthController> logger)
    {
        _logger = logger;
    }

    #endregion

    #region Actions

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateUserRequest model)
    {
        
        return Ok();
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        return Ok();
    }
    #endregion
}
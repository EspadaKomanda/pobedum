using ApiGatewayService.Communicators;
using ApiGatewayService.Models.BasicResponses;
using ApiGatewayService.Models.Microservices.AuthService.Requests.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using LoginRequest = ApiGatewayService.Models.Microservices.AuthService.Requests.Users.LoginRequest;

namespace ApiGatewayService.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    #region Services
    
    private readonly ILogger<AuthController> _logger;
    private readonly AuthCommunicator _authCommunicator;
    #endregion

    #region Constructor

    public AuthController(ILogger<AuthController> logger, AuthCommunicator authCommunicator)
    {
        _logger = logger;
        _authCommunicator = authCommunicator;
    }

    #endregion

    #region Actions

    /*
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateUserRequest model)
    {
        try
        {
            return Ok(await _authCommunicator.SendRegistrationRequest(model));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
            return BadRequest(new BasicResponse()
            {
                Message = e.Message,
                Code = 400
            });
        }
    }
    */
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        try
        {
            return Ok(await (_authCommunicator.SendLoginRequest(model)));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
            return BadRequest(new BasicResponse()
            {
                Code = 400,
                Message = e.Message
            });
        }
    }
    #endregion
}
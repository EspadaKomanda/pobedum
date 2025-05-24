using AuthService.Models.BasicResponses;
using AuthService.Models.Database.Users;
using AuthService.Models.Requests.Users;
using AuthService.Models.Responses.Users;
using AuthService.Services.JWT;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    #region Services
    
    private readonly ILogger<AuthController> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtService _jwtService;
    
    #endregion

    #region Constructor

    public AuthController(ILogger<AuthController> logger, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IJwtService jwtService)
    {
        _logger = logger;
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
    }

    #endregion

    #region Actions

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateUserRequest model)
    {
        try
        {
            var user = new ApplicationUser()
            {
                UserName = model.Username,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);
    
            if (!result.Succeeded)
            {
                _logger.LogError("User registration failed: {Errors}", result.Errors);
                return BadRequest(new BasicResponse
                {
                    Code = 400,
                    Message = "User registration failed"
                });
            }

            _logger.LogInformation("User registered successfully: {Username}", model.Username);
            return Ok(new BasicResponse
            {
                Code = 200,
                Message = "User created successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during user registration: {Message}", ex.Message);
            return StatusCode(500, new BasicResponse
            {
                Code = 500,
                Message = "An error occurred during user registration"
            });
        }
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        try
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            
            if (user == null)
            {
                _logger.LogError("Invalid username or password");
                return NotFound(new BasicResponse
                {
                    Code = 404,
                    Message = "Invalid username or password"
                });
            }
            var result =  await _signInManager.PasswordSignInAsync( user, model.Password, false, model.RememberMe);

            if (result.Succeeded)
            {
                var accessToken = _jwtService.GenerateAccessToken(user);
                _logger.LogInformation("User logged in successfully: {Username}", model.Username);

                return Ok(new LoginResultResponse()
                {
                    Success = true,
                    Token = accessToken
                });
            }
            
            _logger.LogError("Invalid username or password");
            return BadRequest(new BasicResponse
            {
                Code = 400,
                Message = "Invalid username or password"
            });
        
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during user login: {Message}", ex.Message);
            return StatusCode(500, new BasicResponse
            {
                Code = 500,
                Message = "An error occurred during user login"
            });
        }
    }
    
    [Authorize]
    [HttpGet("internal-auth")]
    public async Task<IActionResult> InternalAuth()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("User not found");
        }
        return Ok(new InternalAuthResponse()
        {
            Id = user.Id,
            Role = user.Role.Name
        }); 
    }
    #endregion
}
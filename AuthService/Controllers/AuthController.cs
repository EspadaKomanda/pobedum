using AuthService.Models.Database;
using AuthService.Models.Requests;
using AuthService.Models.Responses.Users;
using AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers;

[ApiController]
[Route("/api/v1/[controller]")]
public class AuthController : ControllerBase
{

    #region Fields

    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IJWTService _jwtService;

    #endregion


    #region Constructor

    public AuthController(UserManager<User> userManager, SignInManager<User> signInManager, IJWTService jwtService, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _roleManager = roleManager;
    }

    #endregion

    #region Actions

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterModel model)
    {
        if (await _userManager.FindByNameAsync(model.Username) != null)
        {
            return BadRequest("Username is already taken");
        }

        if (await _userManager.FindByEmailAsync(model.Email) != null)
        {
            return BadRequest("Email is already registered");
        }

        var user = new User 
        { 
            UserName = model.Username, 
            Email = model.Email,
            Fullname = model.Fullname,
        };
        var result = await _userManager.CreateAsync(user, model.Password);
        
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "User");
            await _signInManager.SignInAsync(user, isPersistent: false);
            return Ok(new { Message = "Registration successful" });
        }
        
        return BadRequest(result.Errors);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginModel model)
    {
        var user = await _userManager.FindByNameAsync(model.UserName);

        if (user == null)
        {
            return Unauthorized("Invalid username or password");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
        
        if (result.Succeeded)
        {
            var token = await _jwtService.GenerateToken(user);
            return Ok(new { Token = token });
        }
        
        return Unauthorized("Invalid username or password");
    }
    
    
    [HttpGet("Test")]
    [Authorize()]
    public async Task<IActionResult> Test()
    {
        
        return Ok();
        
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
            Id = Guid.Parse(user.Id),
            Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault()!
        }); 
    }

    #endregion
}
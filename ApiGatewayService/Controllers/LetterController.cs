using System.Security.Claims;
using ApiGatewayService.Communicators;
using ApiGatewayService.Models.BasicResponses;
using ApiGatewayService.Models.Microservices.Internal;
using ApiGatewayService.Models.Microservices.LetterService.Requests;
using ApiGatewayService.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGatewayService.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class LetterController : ControllerBase
{
    #region Services

    private readonly LettersCommunicator _lettersCommunicator;
    private readonly ILogger<LetterController> _logger;

    #endregion

    #region Constructor

    public LetterController(LettersCommunicator lettersCommunicator, ILogger<LetterController> logger)
    {
        _lettersCommunicator = lettersCommunicator;
        _logger = logger;
    }

    #endregion

    #region Actions

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllLetters([FromQuery] int page, [FromQuery] int size)
    {
        try
        {
            var userId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "userId").Value);
            var userRole = User.Claims.FirstOrDefault(c => c.Type == "userRole").Value;
            return Ok(await _lettersCommunicator.SendGetAllLettersRequest(userId, userRole, page, size));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
            return BadRequest(new BasicResponse()
            {
                Message = e.Message,
                Code = 500
            });
        }
    }
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLetterById(Guid id)
    {
        try
        {
            var userId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultNameClaimType).Value);
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
            return Ok(await _lettersCommunicator.SendGetLetterRequest(userId, userRole, id));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
            return BadRequest(new BasicResponse()
            {
                Message = e.Message,
                Code = 500
            });
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateLetter([FromBody] LetterCreateRequest request)
    {
        try
        {
            var userId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultNameClaimType).Value);
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
            return Ok(await _lettersCommunicator.SendCreateLetterRequest(userId, userRole, new CreateLetterRequest()
            {
                
                Title = request.Title,
                Content = request.Content,
                Date = request.Date,
                Author = new User()
                {
                    Id = userId,
                    Role = userRole
                },
                Resource = request.Resource
            }));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
            throw;
        }
    }
    
    [HttpPost("{id}/favourite")]
    public async Task<IActionResult> AddLetterToFavourites(Guid Id)
    {
        try
        {
            var userId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultNameClaimType).Value);
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
            return Ok(await _lettersCommunicator.SendAddLetterToFavouritesRequest(new AddLetterToFavouritesRequest()
            {
                User = new User()
                {
                    Id = userId,
                    Role = userRole
                }
            }, Id));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
            throw;
        }
    }
    
    [HttpDelete("{id}/favourite")]
    public async Task<IActionResult> DeleteLetterFromFavourites(Guid Id)
    {
        try
        {
            var userId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultNameClaimType).Value);
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultRoleClaimType).Value;
            return Ok(await _lettersCommunicator.SendRemoveLetterToFavouritesRequest(new RemoveLetterFromFavouritesRequest()
            {
                User = new User()
                {
                    Id = userId,
                    Role = userRole
                }
            }, Id));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
            throw;
        }
    }
    
    
    #endregion
}
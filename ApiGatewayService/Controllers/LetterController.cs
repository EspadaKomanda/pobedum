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
    public async Task<IActionResult> GetAllLetters([FromQuery] int page, [FromQuery] int size)
    {
        try
        {
            Guid userIdClaim = Guid.Empty;
            string userRoleClaim = "";
            foreach (var claim in User.Claims)
            {
                if (claim.Type == ClaimTypes.Name)
                    userIdClaim = Guid.Parse(claim.Value);
                if (claim.Type == ClaimTypes.Role)
                    userRoleClaim = claim.Value;
            }

            var result = await _lettersCommunicator.SendGetAllLettersRequest(userIdClaim, userRoleClaim, page, size);

            return Ok(new
            {
                content = result.Letters,
                page = new
                {
                    size = size,
                    number = page,
                    totalElements = result.Letters.Count,
                    totalPages = result.TotalCount
                }
            });
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
    public async Task<IActionResult> GetLetterById(string id)
    {
        try
        {
            Guid userId = Guid.Empty;
            string userRole = "";
            foreach (var claim in User.Claims)
            {
                if(claim.Type==ClaimTypes.Name)
                    userId = Guid.Parse(claim.Value);
                if(claim.Type==ClaimTypes.Role)
                    userRole = claim.Value;   
            }
            return Ok(await _lettersCommunicator.SendGetLetterRequest(userId, userRole, Guid.Parse(id)));
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
            Guid userId = Guid.Empty;
            string userRole = "";
            foreach (var claim in User.Claims)
            {
                if(claim.Type==ClaimTypes.Name)
                    userId = Guid.Parse(claim.Value);
                if(claim.Type==ClaimTypes.Role)
                    userRole = claim.Value;   
            }
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
    public async Task<IActionResult> AddLetterToFavourites([FromRoute] string id)
    {
        try
        {
            Guid userId = Guid.Empty;
            string userRole = "";
            foreach (var claim in User.Claims)
            {
                if(claim.Type==ClaimTypes.Name)
                    userId = Guid.Parse(claim.Value);
                if(claim.Type==ClaimTypes.Role)
                    userRole = claim.Value;   
            }
            return Ok(await _lettersCommunicator.SendAddLetterToFavouritesRequest(new AddLetterToFavouritesRequest()
            {
                User = new User()
                {
                    Id = userId,
                    Role = userRole
                }
            }, Guid.Parse(id)));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
            throw;
        }
    }
    
    [HttpDelete("{id}/favourite")]
    public async Task<IActionResult> DeleteLetterFromFavourites(string id)
    {
        try
        {
            Guid userId = Guid.Empty;
            string userRole = "";
            foreach (var claim in User.Claims)
            {
                if(claim.Type==ClaimTypes.Name)
                    userId = Guid.Parse(claim.Value);
                if(claim.Type==ClaimTypes.Role)
                    userRole = claim.Value;   
            }
            return Ok(await _lettersCommunicator.SendRemoveLetterToFavouritesRequest(new RemoveLetterFromFavouritesRequest()
            {
                User = new User()
                {
                    Id = userId,
                    Role = userRole
                }
            }, Guid.Parse(id)));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
            throw;
        }
    }
    
    
    #endregion
}
using System.Security.Claims;
using ApiGatewayService.Communicators;
using ApiGatewayService.Models.BasicResponses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace ApiGatewayService.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class VideoController : ControllerBase
{
    #region Fields

    private readonly VideosCommunicator _videosCommunicator;
    private readonly ILogger<VideoController> _logger;

    #endregion

    #region Constructor

    public VideoController(VideosCommunicator videosCommunicator, ILogger<VideoController> logger)
    {
        _videosCommunicator = videosCommunicator;
        _logger = logger;
    }

    #endregion

    #region Actions

    [HttpGet("{id}")]
    public async Task<IActionResult> GetVideoById(Guid id)
    {
        try
        {
           
            return Ok(await _videosCommunicator.SendGetVideoByIdRequest(id));
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

    [HttpGet("videos")]
    public async Task<IActionResult> GetVideoByUserId([FromQuery] int page, [FromQuery] int size)
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

            if (userId != Guid.Empty)
            {
                
                return Ok(await _videosCommunicator.SendGetVideoByUserId(userId,page,size));
            }
            return Unauthorized();
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
    
    #endregion
}
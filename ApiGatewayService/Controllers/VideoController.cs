using ApiGatewayService.Communicators;
using ApiGatewayService.Models.BasicResponses;
using Microsoft.AspNetCore.Mvc;

namespace ApiGatewayService.Controllers;

[ApiController]
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

    [HttpGet("videos/{userId}")]
    public async Task<IActionResult> GetVideoByUserId(Guid userId,[FromQuery] int page, [FromQuery] int size)
    {
        try
        {
            return Ok(await _videosCommunicator.SendGetVideoByUserId(userId,page,size));
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
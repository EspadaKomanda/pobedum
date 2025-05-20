using System.Security.Claims;
using ApiGatewayService.Communicators;
using ApiGatewayService.Models.BasicResponses;
using ApiGatewayService.Models.Microservices.PipelineService.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGatewayService.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class VideoGenerationController : ControllerBase
{
    #region Services

    private readonly VideoGenerationCommunicator _videoGenerationCommunicator;
    private readonly ILogger<VideoGenerationController> _logger;

    #endregion

    #region Constructor

    public VideoGenerationController(VideoGenerationCommunicator videoGenerationCommunicator, ILogger<VideoGenerationController> logger)
    {
        _videoGenerationCommunicator = videoGenerationCommunicator;
        _logger = logger;
    }

    #endregion

    #region Actions

    [HttpPost]
    public async Task<IActionResult> BeginVideoGeneration([FromBody] BeginVideoGenerationRequest request)
    {
        try
        {
            return Ok(await _videoGenerationCommunicator.SendBeginVideoGenerationRequest(Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultNameClaimType).Value), request)); 
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

    [HttpGet("status/{taskId}")]
    public async Task<IActionResult> GetTaskStatus(Guid taskId)
    {
        try
        {
            return Ok(await _videoGenerationCommunicator.SendGetStatusRequest(taskId)); 
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
    
    [HttpGet("queue-position/{taskId}")]
    public async Task<IActionResult> GetQueuePosition(Guid taskId)
    {
        try
        {
            return Ok(await _videoGenerationCommunicator.SendGetQueuePositionRequest(taskId));
        }
        catch (Exception e)
        {
            _logger.LogError(e,e.Message);
            throw;
        }
    }
    #endregion
}
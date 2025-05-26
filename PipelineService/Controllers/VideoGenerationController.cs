using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PipelineService.Models.BasicResponses;
using PipelineService.Models.Requests;
using PipelineService.Services.Pipeline;

namespace PipelineService.Controllers;

[ApiController]
[Route("/api/v1/[controller]")]
public class VideoGenerationController : ControllerBase
{
    #region Services
    
    private readonly ILogger<VideoGenerationController> _logger;
    private readonly IPipelineService _pipelineService;

    #endregion

    #region Constructor

    public VideoGenerationController(ILogger<VideoGenerationController> logger, IPipelineService pipelineService)
    {
        _logger = logger;
        _pipelineService = pipelineService;
    }

    #endregion

    #region Actions

    [HttpPost]
    public async Task<IActionResult> BeginGeneration([FromHeader] Guid userId,[FromBody] BeginVideoGenerationRequest request)
    {
        try
        {
            return Ok(await _pipelineService.BeginVideoGeneration(request,userId));
        }
        catch (Exception e)
        {
            _logger.LogError(e,e.Message);
            throw;
        }
    }

    [HttpGet("status/{taskId}")]
    public async Task<IActionResult> GetTaskStatus(Guid taskId)
    {
        try
        {
            return Ok(await _pipelineService.GetStatus(taskId));
        }
        catch (Exception e)
        {
            _logger.LogError(e,e.Message);
            throw;
        }
    }
    [HttpGet("queue-position/{taskId}")]
    public IActionResult GetQueuePosition(Guid taskId)
    {
        try
        {
            return Ok( new BasicResponse()
            {
                Message = _pipelineService.GetQueuePosition(taskId).ToString(),
                Code = 200
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e,e.Message);
            throw;
        }
    }
    #endregion
    
    
}
using Microsoft.AspNetCore.Mvc;
using VideoService.Models.BasicResponses;
using VideoService.Models.Internal;
using VideoService.Models.Internal.Requests;
using VideoService.Services.Videos;

namespace VideoService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VideoController : ControllerBase
{
    #region Fields

    private readonly IVideoService _videoService;
    private readonly ILogger<VideoController> _logger;
    
    #endregion

    #region Constructor

    public VideoController(IVideoService videoService, ILogger<VideoController> logger)
    {
        _videoService = videoService;
        _logger = logger;
    }

    #endregion

    #region Action

    [HttpGet("{videoId}")]
    public async Task<IActionResult> GetVideoById(Guid videoId)
    {
        try
        {
            return Ok(await _videoService.GetVideoByIdAsync(videoId));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message,e);
            return BadRequest(new BasicResponse()
            {
                Message = "Получение видео пошло не по плану:)",
                Code = 400
            });
        }
    }
    
    [HttpGet("videos/{userId}")]
    public async Task<IActionResult> GetVideoByUserId(Guid userId,[FromQuery] int page, [FromQuery] int size)
    {
        try
        {
            return Ok(await _videoService.GetVideosByUserId(new User(){Id = userId}, page, size));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message,e);
            return BadRequest(new BasicResponse()
            {
                Message = "Получение видео пошло не по плану:)",
                Code = 400
            });
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> AddVideo(AddVideoRequest request)
    {
        try
        {
            return Ok(await _videoService.AddVideo(request));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message,e);
            return BadRequest(new BasicResponse()
            {
                Message = "Добавление видео пошло не по плану:)",
                Code = 400
            });
        }
    }
    #endregion
}
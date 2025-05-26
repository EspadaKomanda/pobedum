using LetterService.Models.BasicResponses;
using LetterService.Models.Internal;
using LetterService.Models.Requests;
using LetterService.Services.Favourites;
using LetterService.Services.Letters;
using Microsoft.AspNetCore.Mvc;

namespace LetterService.Controllers;

[ApiController]
[Route("/api/v1/[controller]")]
public class LetterController : ControllerBase
{
    #region Fields

    private readonly ILogger<LetterController> _logger;
    private readonly ILetterService _letterService;
    private readonly IFavouritesService _favouritesService;

    #endregion

    #region Constructor

    public LetterController(ILogger<LetterController> logger, ILetterService letterService, IFavouritesService favouritesService)
    {
        _logger = logger;
        _letterService = letterService;
        _favouritesService = favouritesService;
    }

    #endregion

    #region Actions


    #region Letters

    [HttpGet]
    public async Task<IActionResult> GetLetters([FromHeader]Guid? userId, [FromHeader]string? userRole,[FromQuery] int page, [FromQuery] int size)
    {
        try
        {
            if (userId != null && userRole != null)
            {
                return Ok(await _letterService.GetAllLettersAsync(new User(){Id = (Guid)userId,Role = userRole},page,size));

            }

            return Ok(await _letterService.GetAllLettersAsync(page,size));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message,e);
            return BadRequest(new BasicResponse()
            {
                Message = "Получение писем пошло не по плану:)",
                Code = 400
            });
        }
    }
    
    [HttpGet("{letterId}")]
    public async Task<IActionResult> GetLetter([FromHeader]Guid? userId, [FromHeader]string userRole, Guid letterId)
    {
        try
        {
            if (userId != null && userRole != null)
            {
                return Ok(await _letterService.GetLetterByIdAsync(new User(){Id = (Guid)userId,Role = userRole},letterId));
            }

            return Ok(await _letterService.GetLetterByIdAsync(letterId));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message,e);
            return BadRequest(new BasicResponse()
            {
                Message = "Получение писем пошло не по плану:)",
                Code = 400
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateLetter([FromBody] CreateLetterRequest request)
    {
        try
        {
            return StatusCode(201, await _letterService.CreateLetterAsync(request));
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message,e);
            return BadRequest(request);
        }
    }

    #endregion

    #region Favourites

    [HttpPost("{letterId}/favourite")]
    public async Task<IActionResult> AddLetterToFavourites([FromBody] AddLetterToFavouritesRequest request, Guid letterId)
    {
        try
        {
            return Ok(new BasicResponse()
            {
                Message = (await _favouritesService.AddLetterToFavouritesAsync(request,letterId)).ToString(),
                Code = 200
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message,e);
            return NotFound(new BasicResponse()
            {
                Code = 404,
                Message = "Письмо не найдено -_-"
            });
        }
    }
    [HttpDelete("{letterId}/favourite")]
    public async Task<IActionResult> RemoveLetterFromFavourites([FromBody] RemoveLetterFromFavouritesRequest request, Guid letterId)
    {
        try
        {
            return Ok(new BasicResponse()
            {
                Message = (await _favouritesService.RemoveLetterFromFavouritesAsync(request, letterId)).ToString(),
                Code = 200
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message,e);
            return NotFound(new BasicResponse()
            {
                Code = 404,
                Message = "Письмо не найдено -_-"
            });
        }
    }
    #endregion
    
    #endregion
}
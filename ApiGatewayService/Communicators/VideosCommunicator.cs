using ApiGatewayService.Models.Microservices.VideoService;
using ApiGatewayService.Models.Microservices.VideoService.DTOs;
using ApiGatewayService.Utils;

namespace ApiGatewayService.Communicators;

public class VideosCommunicator
{
    #region Fields

    private readonly ILogger<VideosCommunicator> _logger;
    private readonly MicroservicesHttpClient _microservicesHttpClient;
    private readonly IConfiguration _configuration;
    private Dictionary<string, string> _paths; 
        
    #endregion

    #region Constructor

    public VideosCommunicator(ILogger<VideosCommunicator> logger, MicroservicesHttpClient microservicesHttpClient, IConfiguration configuration)
    {
        _logger = logger;
        _microservicesHttpClient = microservicesHttpClient;
        _configuration = configuration;
        _paths = new Dictionary<string, string>();
        ConfigurePaths();
    }

    #endregion

    #region Methods

    private void ConfigurePaths()
    {
        var authPaths = _configuration.GetSection("VideoServicePaths");
        foreach (var path in authPaths.GetChildren())
        {
            _paths.Add(path.Key,path.Value);
        }
    }
    
    public async Task<VideoDTO> SendGetVideoByIdRequest(Guid videoId)
    {
        try
        {
            return await _microservicesHttpClient.GetAsync<VideoDTO>($"{_paths["GetVideoById"]}/{videoId}",
                new Dictionary<string, string>());
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message,e);
            throw;
        }
    }
    
    public async Task<GetVideosByUserIdResponse> SendGetVideoByUserId(Guid userId, int page, int size)
    {
        try
        {
            return await _microservicesHttpClient.GetAsync<GetVideosByUserIdResponse>($"{_paths["GetVideoByUserId"]}/{userId}?page={page}&size={size}",
                new Dictionary<string, string>());
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message,e);
            throw;
        }
    }
    
    #endregion
}
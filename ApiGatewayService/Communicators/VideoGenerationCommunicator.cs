using ApiGatewayService.Models.BasicResponses;
using ApiGatewayService.Models.Microservices.LetterService.DTOs;
using ApiGatewayService.Models.Microservices.PipelineService.Requests;
using ApiGatewayService.Models.Microservices.PipelineService.Responses;
using ApiGatewayService.Utils;

namespace ApiGatewayService.Communicators;

public class VideoGenerationCommunicator
{
    #region Fields

    private readonly ILogger<AuthCommunicator> _logger;
    private readonly MicroservicesHttpClient _microservicesHttpClient;
    private readonly IConfiguration _configuration;
    private Dictionary<string, string> _paths; 
        
    #endregion

    #region Constructor

    public VideoGenerationCommunicator(ILogger<AuthCommunicator> logger, MicroservicesHttpClient microservicesHttpClient, IConfiguration configuration)
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
        var authPaths = _configuration.GetSection("VideoGenerationServicePaths");
        foreach (var path in authPaths.GetChildren())
        {
            _paths.Add(path.Key,path.Value);
        }
    }

    public async Task<GetStatusResponse> SendGetStatusRequest(Guid taskId)
    {
        try
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            return await _microservicesHttpClient.GetAsync<GetStatusResponse>($"{_paths["GetStatus"]}/{taskId}",headers);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message,e);
            throw;
        }
    }
    
    public async Task<int> SendGetQueuePositionRequest(Guid taskId)
    {
        try
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            return await _microservicesHttpClient.GetAsync<int>($"{_paths["GetQueuePosition"]}/{taskId}",headers);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message,e);
            throw;
        }
    }
    

    public async Task<BeginVideoGenerationResponse> SendBeginVideoGenerationRequest(Guid userId, BeginVideoGenerationRequest request)
    {
        try
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
           
            headers.Add("userId",userId.ToString());

            return await _microservicesHttpClient.PostAsync<BeginVideoGenerationResponse>($"{_paths["BeginVideoGeneration"]}",headers,request);

        }
        catch (Exception e)
        {
            _logger.LogError(e.Message,e);
            throw;
        }
    }
    #endregion
}
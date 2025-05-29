using PipelineService.Models.Internal.Requests;
using PipelineService.Utils;

namespace PipelineService.Communicators;

public class VideosCringeCommunicator
{
    #region Fields

    private readonly MicroservicesCringeHttpClient _microservicesHttpClient;
    private readonly IConfiguration _configuration;
    private Dictionary<string, string> _paths; 
        
    #endregion

    #region Constructor

    public VideosCringeCommunicator(MicroservicesCringeHttpClient microservicesHttpClient, IConfiguration configuration)
    {
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
    
    public async Task<bool> SendAddVideoRequest(AddVideoRequest request)
    {
        try
        {
            return await _microservicesHttpClient.PostAsync<bool>($"{_paths["AddVideo"]}",
                new Dictionary<string, string>(),request);
        }
        catch (Exception e)
        {
            throw;
        }
    }
    
    
    #endregion
}
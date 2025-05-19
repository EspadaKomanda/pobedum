using ApiGatewayService.Models.BasicResponses;
using ApiGatewayService.Models.Microservices.AuthService.Requests.Users;
using ApiGatewayService.Models.Microservices.AuthService.Responses.Users;
using ApiGatewayService.Utils;

namespace ApiGatewayService.Communicators;

public class AuthCommunicator
{
    #region Fields

    private readonly ILogger<AuthCommunicator> _logger;
    private readonly MicroservicesHttpClient _microservicesHttpClient;
    private readonly IConfiguration _configuration;
    private Dictionary<string, string> _paths; 
        
    #endregion

    #region Constructor

    public AuthCommunicator(ILogger<AuthCommunicator> logger, MicroservicesHttpClient microservicesHttpClient, IConfiguration configuration)
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
        var authPaths = _configuration.GetSection("AuthServicePaths");
        foreach (var path in authPaths.GetChildren())
        {
            _paths.Add(path.Key,path.Value);
        }
    }
    
    public async Task<BasicResponse> SendRegistrationRequest(CreateUserRequest request)
    {
        try
        {
            return await _microservicesHttpClient.PostAsync<BasicResponse>(_paths["Registration"],new Dictionary<string, string>(),request);
        }   
        catch (Exception e)
        {
            _logger.LogError(e.Message,e);
            throw;
        }
    }

    public async Task<LoginResultResponse> SendLoginRequest(LoginRequest request)
    {
        try
        {
            return await _microservicesHttpClient.PostAsync<LoginResultResponse>(_paths["Login"],new Dictionary<string, string>(),request);
        } 
        catch (Exception e)
        {
            _logger.LogError(e.Message,e);
            throw;
        }
    }

    public async Task<InternalAuthResponse> HandleInternalAuth(string token)
    {
        try
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Authorization",token);
            return await _microservicesHttpClient.GetAsync<InternalAuthResponse>(_paths["InternalAuth"],headers);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message,e);
            throw;
        }
    }
    #endregion
}
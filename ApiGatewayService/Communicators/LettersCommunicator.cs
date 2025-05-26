using ApiGatewayService.Models.BasicResponses;
using ApiGatewayService.Models.Microservices.LetterService.DTOs;
using ApiGatewayService.Models.Microservices.LetterService.Requests;
using ApiGatewayService.Models.Microservices.LetterService.Responses;
using ApiGatewayService.Utils;

namespace ApiGatewayService.Communicators;

public class LettersCommunicator
{
    #region Fields

    private readonly ILogger<LettersCommunicator> _logger;
    private readonly MicroservicesHttpClient _microservicesHttpClient;
    private readonly IConfiguration _configuration;
    private Dictionary<string, string> _paths; 
        
    #endregion

    #region Constructor

    public LettersCommunicator(ILogger<LettersCommunicator> logger, MicroservicesHttpClient microservicesHttpClient, IConfiguration configuration)
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
        var authPaths = _configuration.GetSection("LettersServicePaths");
        foreach (var path in authPaths.GetChildren())
        {
            _paths.Add(path.Key,path.Value);
        }
    }

    public async Task<GetLettersRequest> SendGetAllLettersRequest(Guid? userId,
        string? userRole, int page, int size)
    {
        try
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            if (userId != null && userRole != null)
            {  
                headers.Add("userId",userId.ToString());
                headers.Add("userRole",userRole);
            }
            string url = ($"{_paths["GetAllLetters"]}?page={page}&size={size}");
            Console.WriteLine(page);
            Console.WriteLine(size);
            return await _microservicesHttpClient.GetAsync<GetLettersRequest>(url,headers);

        }
        catch (Exception e)
        {
            _logger.LogError(e.Message,e);
            throw;
        }
    }
    
    public async Task<LetterDTO> SendGetLetterRequest(Guid? userId, string? userRole, Guid letterId)
    {
        try
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            if (userId != null && userRole != null)
            {  
                headers.Add("userId",userId.ToString());
                headers.Add("userRole",userRole);
            }

            return await _microservicesHttpClient.GetAsync<LetterDTO>($"{_paths["GetAllLetters"]}/{letterId}",headers);

        }
        catch (Exception e)
        {
            _logger.LogError(e.Message,e);
            throw;
        }
    }

    public async Task<LetterDTO> SendCreateLetterRequest(Guid? userId, string? userRole, CreateLetterRequest request)
    {
        try
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            if (userId != null && userRole != null)
            {  
                headers.Add("userId",userId.ToString());
                headers.Add("userRole",userRole);
            }

            return await _microservicesHttpClient.PostAsync<LetterDTO>($"{_paths["GetAllLetters"]}",headers,request);

        }
        catch (Exception e)
        {
            _logger.LogError(e.Message,e);
            throw;
        }
    }

    public async Task<BasicResponse> SendAddLetterToFavouritesRequest(AddLetterToFavouritesRequest request, Guid letterId)
    {
        try
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();

            return await _microservicesHttpClient.PostAsync<BasicResponse>($"{_paths["GetAllLetters"]}/{letterId}/favourite",headers,request);

        }
        catch (Exception e)
        {
            _logger.LogError(e.Message,e);
            throw;
        }
    }
    
    public async Task<BasicResponse> SendRemoveLetterToFavouritesRequest(RemoveLetterFromFavouritesRequest request, Guid letterId)
    {
        try
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();

            return await _microservicesHttpClient.DeleteAsync<BasicResponse>($"{_paths["GetAllLetters"]}/{letterId}/favourite",headers,request);

        }
        catch (Exception e)
        {
            _logger.LogError(e.Message,e);
            throw;
        }
    }
    #endregion
}
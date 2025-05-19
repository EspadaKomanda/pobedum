using System.Diagnostics;
using System.Net;
using ApiGatewayService.Exceptions.HttpClient;
using Newtonsoft.Json;

namespace ApiGatewayService.Utils;

public class MicroservicesHttpClient
{
    #region Fields

    private readonly HttpClient _httpClient;
    private readonly ILogger<MicroservicesHttpClient> _logger;
    #endregion

    #region Constructor

    public MicroservicesHttpClient(HttpClient httpClient, ILogger<MicroservicesHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    #endregion

    #region Methods

    public async Task<T> GetAsync<T>(string url, Dictionary<string,string> headers)
    {
        try
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(url);
            request.Method = HttpMethod.Get;
            foreach (var header in headers)
            {
                request.Headers.Add(header.Key,header.Value);
            }

            return await SendRequest<T>(request);

        }
        catch (Exception e)
        {
            _logger.LogError(e.Message,e);
            throw;
        }
    }
    
    public async Task<T> PostAsync<T>(string url, Dictionary<string,string> headers, object body)
    {
        try
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(url);
            request.Method = HttpMethod.Post;
            request.Content = new StringContent(JsonConvert.SerializeObject(body));
            foreach (var header in headers)
            {
                request.Headers.Add(header.Key,header.Value);
            }

            return await SendRequest<T>(request);

        }
        catch (Exception e)
        {
            _logger.LogError(e.Message,e);
            throw;
        }
    }

    public async Task<T> DeleteAsync<T>(string url, Dictionary<string,string> headers, object body)
    {
        try
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(url);
            request.Method = HttpMethod.Delete;
            request.Content = new StringContent(JsonConvert.SerializeObject(body));
            foreach (var header in headers)
            {
                request.Headers.Add(header.Key,header.Value);
            }

            return await SendRequest<T>(request);

        }
        catch (Exception e)
        {
            _logger.LogError(e.Message,e);
            throw;
        }
    }

    private async Task<T> SendRequest<T>(HttpRequestMessage request)
    {
        var response = await _httpClient.SendAsync(request);
            
        if (response.StatusCode is HttpStatusCode.Created or HttpStatusCode.OK)
        {
            return await response.Content.ReadFromJsonAsync<T>();
        }
        else if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new BadRequestException(await response.Content.ReadAsStringAsync());
        }
        else if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedException(await response.Content.ReadAsStringAsync());
        }
        else if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new NotFoundException(await response.Content.ReadAsStringAsync());
        }

        throw new UnreachableException(await response.Content.ReadAsStringAsync());
    }
    #endregion
}
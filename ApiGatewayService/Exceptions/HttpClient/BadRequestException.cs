namespace ApiGatewayService.Exceptions.HttpClient;

public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message) { }
}
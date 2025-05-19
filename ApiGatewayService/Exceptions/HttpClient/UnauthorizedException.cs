namespace ApiGatewayService.Exceptions.HttpClient;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
}
namespace PipelineService.Exceptions.HttpClient;

public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message) { }
}
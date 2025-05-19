namespace PipelineService.Exceptions.HttpClient;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
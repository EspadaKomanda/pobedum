namespace PipelineService.Autumn.Exceptions.ConsumerExceptions;

public class ConsumerTopicUnavailableException : ConsumerException
{
    public ConsumerTopicUnavailableException()
    {
    }

    public ConsumerTopicUnavailableException(string message)
        : base(message)
    {
    }

    public ConsumerTopicUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
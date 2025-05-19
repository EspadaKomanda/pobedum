namespace PipelineService.Autumn.Exceptions;

public class KafkaException : Exception
{
    public KafkaException()
    {
    }

    public KafkaException(string message)
        : base(message)
    {
    }

    public KafkaException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
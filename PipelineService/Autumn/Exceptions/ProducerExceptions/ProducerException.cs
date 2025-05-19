namespace PipelineService.Autumn.Exceptions.ProducerExceptions;

public class ProducerException : KafkaException
{
    public ProducerException()
    {
        
    }

    public ProducerException(string message)
        : base(message)
    {
        
    }

    public ProducerException(string message, Exception innerException)
        : base(message, innerException)
    {
        
    }
}
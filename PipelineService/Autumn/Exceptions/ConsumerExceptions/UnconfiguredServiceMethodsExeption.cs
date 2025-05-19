namespace PipelineService.Autumn.Exceptions.ConsumerExceptions
{
    public class UnconfiguredServiceMethodsException : ConsumerException
    {
        public UnconfiguredServiceMethodsException()
        {
        }

        public UnconfiguredServiceMethodsException(string message)
            : base(message)
        {
        }

        public UnconfiguredServiceMethodsException(string message, Exception innerException)
            : base(message, innerException)
        {
    }
    }
}
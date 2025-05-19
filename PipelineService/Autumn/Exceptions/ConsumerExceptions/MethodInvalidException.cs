namespace PipelineService.Autumn.Exceptions.ConsumerExceptions
{
    public class MethodInvalidException : ConsumerException
    {
        public MethodInvalidException()
        {
        }

        public MethodInvalidException(string message)
            : base(message)
        {
        }

        public MethodInvalidException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
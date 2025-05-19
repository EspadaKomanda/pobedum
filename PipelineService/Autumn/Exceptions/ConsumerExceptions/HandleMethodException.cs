namespace PipelineService.Autumn.Exceptions.ConsumerExceptions
{
    public class HandleMethodException : ConsumerException
    {
        public HandleMethodException()
        {
        }

        public HandleMethodException(string message)
            : base(message)
        {
        }

        public HandleMethodException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
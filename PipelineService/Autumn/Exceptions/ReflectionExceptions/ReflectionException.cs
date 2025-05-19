namespace PipelineService.Autumn.Exceptions.ReflectionExceptions
{
    public class ReflectionException : Exception
    {
        public ReflectionException()
        {}

        public ReflectionException(string message)
            : base(message)
        {}

        public ReflectionException(string message, Exception innerException)
            : base(message, innerException)
        {}
    }
}
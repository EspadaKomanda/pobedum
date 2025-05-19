namespace PipelineService.Autumn.Exceptions.ConsumerExceptions
{
    public class ConfigInvalidException : ConsumerException
    {
        public ConfigInvalidException()
        {
        }

        public ConfigInvalidException(string message)
            : base(message)
        {
        }

        public ConfigInvalidException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
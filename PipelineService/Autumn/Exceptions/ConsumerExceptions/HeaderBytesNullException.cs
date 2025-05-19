namespace PipelineService.Autumn.Exceptions.ConsumerExceptions
{
    public class HeaderBytesNullException : ConsumerException
    {
        public HeaderBytesNullException()
        {
        }

        public HeaderBytesNullException(string message)
            : base(message)
        {
        }

        public HeaderBytesNullException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
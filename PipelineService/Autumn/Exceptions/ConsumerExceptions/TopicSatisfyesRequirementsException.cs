namespace PipelineService.Autumn.Exceptions.ConsumerExceptions
{
    public class TopicSatisfiesRequirementsException : ConsumerException
    {
        public TopicSatisfiesRequirementsException()
        {
        }

        public TopicSatisfiesRequirementsException(string message)
            : base(message)
        {
        }

        public TopicSatisfiesRequirementsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
namespace PipelineService.Autumn.Exceptions.ConfigurationException
{
    public class ConfigureConsumersException : KafkaException
    {
        public ConfigureConsumersException() {}
        public ConfigureConsumersException(string message) : base(message) {}
        public ConfigureConsumersException(string message, System.Exception inner) : base(message, inner) {}
    }
}
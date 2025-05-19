namespace PipelineService.Autumn.Exceptions.ConfigurationException
{
   public class ConfigureMessageBusException : KafkaException
   {
      public ConfigureMessageBusException() {}
      public ConfigureMessageBusException(string message) : base(message) {}
      public ConfigureMessageBusException(string message, System.Exception inner) : base(message, inner) {}
   }
}
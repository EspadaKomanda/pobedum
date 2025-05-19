namespace PipelineService.Autumn.Utils.Models
{
    public class MessageHandlerConfig
    {
        public TopicConfig RequestTopicConfig {get;set;} = null!;
        public MessageHandlerType MessageHandlerType { get; set; }
        public HashSet<KafkaMethodExecutionConfig> kafkaMethodExecutionConfigs {get;set;} = null!;
    }
}
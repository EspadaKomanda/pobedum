namespace PipelineService.Autumn.Utils.Models
{
    public class KafkaMethodExecutionConfig
    {
        public string KafkaMethodName { get; set; } = null!;
        public ServiceMethodPair ServiceMethodPair { get; set; } = null!;
        public bool RequireResponse {get;set;}
        public TopicConfig? responseTopicConfig { get; set; }
        public int? responseTopicPartition {get;set;}
        public string? KafkaServiceName {get; set;}
    }
}
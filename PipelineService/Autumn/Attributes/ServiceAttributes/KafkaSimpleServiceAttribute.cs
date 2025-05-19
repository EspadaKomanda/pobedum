using PipelineService.Autumn.Utils.Models;

namespace PipelineService.Autumn.Attributes.ServiceAttributes
{
    //TODO: Validators inside MessageHandlers factory, to verify topics, partitions, methods and etc.
    /// <summary>
    /// Attribute for simple service with request and response topics with one partition,
    /// methods has to be annotated with 'KafkaMethodAttribute',
    /// if responding to requests is not required leave responseTopic null
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class KafkaSimpleServiceAttribute : Attribute
    {
        #region Fields
        
        private readonly string _requestTopicName;
        private readonly int _requestPartitionsCount;
        private readonly short _requestReplicationFactor;
        private readonly string? _responseTopicName;
        private readonly int? _responsePartitionsCount;
        private readonly short? _responseReplicationFactor;
        private readonly string _kafkaServiceName;
        private readonly MessageHandlerType _messageHandlerType;
        private readonly int? _responsePartition; 
        
        #endregion
        
        
        #region Properties

        public TopicConfig RequestTopic => new TopicConfig() { TopicName = _requestTopicName, PartitionsCount = _requestPartitionsCount, ReplicationFactor = _requestReplicationFactor};

        public TopicConfig? ResponseTopic => new TopicConfig(){ PartitionsCount = _requestPartitionsCount, ReplicationFactor = _requestReplicationFactor, TopicName = _requestTopicName};

        public string KafkaServiceName => _kafkaServiceName;
        public MessageHandlerType MessageHandlerType => _messageHandlerType;
        public int? ResponsePartition => _responsePartition;
        
        #endregion
        #region Constructor
        public KafkaSimpleServiceAttribute(
            string requestTopicName, 
            int requestPartitionsCount, 
            short requestReplicationFactor,
            MessageHandlerType messageHandlerType, 
            string kafkaServiceName)
        {
            _requestTopicName = requestTopicName;
            _requestPartitionsCount = requestPartitionsCount;
            _requestReplicationFactor = requestReplicationFactor;
            _messageHandlerType = messageHandlerType;
            _kafkaServiceName = kafkaServiceName;
        }
        public KafkaSimpleServiceAttribute(
            string requestTopicName, 
            int requestPartitionsCount, 
            short requestReplicationFactor,
            MessageHandlerType messageHandlerType, 
            string kafkaServiceName, 
            int? responsePartition = null,
            string? responseTopicName = null,
            int? responsePartitionsCount = null,
            short? responseReplicationFactor = null)
        {
            _requestTopicName = requestTopicName;
            _requestPartitionsCount = requestPartitionsCount;
            _requestReplicationFactor = requestReplicationFactor;
            _responseTopicName = responseTopicName;
            _responsePartitionsCount = responsePartitionsCount;
            _responseReplicationFactor = responseReplicationFactor;
            _messageHandlerType = messageHandlerType;
            _kafkaServiceName = kafkaServiceName;
            _responsePartition = responsePartition;
        }
        
        #endregion
    }
}
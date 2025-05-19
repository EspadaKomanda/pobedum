namespace PipelineService.Autumn.Utils.Models
{
    public class TopicConfig
    {
        public TopicConfig()
        {
        }

        public TopicConfig(int partitionsCount, short replicationFactor, string topicName)
        {
            PartitionsCount = partitionsCount;
            ReplicationFactor = replicationFactor;
            TopicName = topicName;
        }

        public string TopicName { get; set; } = null!;
        public int PartitionsCount {get;set;}
        public short ReplicationFactor {get;set;} = 1;
    }
}
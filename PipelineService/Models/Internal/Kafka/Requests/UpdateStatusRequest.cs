using PipelineService.Utils;

namespace PipelineService.Models.Internal.Kafka.Requests;

public class UpdateStatusRequest
{
    public GenerationStatuses Status { get; set; }
    public Guid TaskId { get; set; }
}
using PipelineService.Utils;

namespace PipelineService.Models.Internal.Kafka.Requests;

public class UpdateStatusRequest
{
    public Guid TaskId { get; set; }
    public GenerationStatuses Status { get; set; }
}
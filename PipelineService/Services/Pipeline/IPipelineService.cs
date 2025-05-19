using PipelineService.Models.Internal.Kafka.Requests;
using PipelineService.Models.Requests;
using PipelineService.Models.Responses;

namespace PipelineService.Services.Pipeline;

public interface IPipelineService
{
    Task<BeginVideoGenerationResponse> BeginVideoGeneration(BeginVideoGenerationRequest request, Guid userId);
    Task UpdateStatus(UpdateStatusRequest request);
    Task<GetStatusResponse> GetStatus(Guid taskId);
    int GetQueuePosition(Guid taskId);
}
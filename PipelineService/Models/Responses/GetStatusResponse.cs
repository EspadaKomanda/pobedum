using PipelineService.Utils;

namespace PipelineService.Models.Responses;

public class GetStatusResponse
{
    public GenerationStatuses Status { get; set; }
    public int Progres { get; set; }
    public int Eta { get; set; }
}
using PipelineService.Utils;

namespace PipelineService.Models.Responses;

public class GetStatusResponse
{
    public string Status { get; set; }
    public int Progres { get; set; }
    public int Eta { get; set; }
}
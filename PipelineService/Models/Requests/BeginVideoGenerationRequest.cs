namespace PipelineService.Models.Requests;

public class BeginVideoGenerationRequest
{
    public Guid TaskId { get; set; }
    public string Text { get; set; }
    public string ColorScheme { get; set; }
    public string Resolution { get; set; }
    public string Model { get; set; }
    public int FrameRate { get; set; }
}
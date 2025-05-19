namespace PipelineService.Models.Internal.Kafka.Requests;

public class BeginGenerationRequest
{
    public Guid PipelineId { get; set; }
    public Guid VideoId { get; set; }
    public string Text { get; set; }
    public string ColorScheme { get; set; }
    public string Resolution { get; set; }
    public string Model { get; set; }
    public int FrameRate { get; set; }
}
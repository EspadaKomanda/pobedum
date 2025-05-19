using System.ComponentModel.DataAnnotations;
using PipelineService.Utils;

namespace PipelineService.Models.Database;

public class PipelineItem
{
    [Key]
    public Guid Id { get; set; }
    public Guid VideoId { get; set; }
    public Guid UserId { get; set; }
    public string Text { get; set; }
    public string ColorScheme { get; set; }
    public string Resolution { get; set; }
    public string Model { get; set; }
    public int FrameRate { get; set; }
    public GenerationStatuses Status { get; set; }
    public DateTime BeginTime { get; set; }
    
    public DateTime? EndTime { get; set; }
}
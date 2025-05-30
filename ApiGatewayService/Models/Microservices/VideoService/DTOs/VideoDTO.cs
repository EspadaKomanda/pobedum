namespace ApiGatewayService.Models.Microservices.VideoService.DTOs;

public class VideoDTO
{
    public Guid VideoId { get; set; }
    public string VideoUrl { get; set; }
    public string Text { get; set; }
    public string? Resolution { get; set; }
    public int? DurationSeconds { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? SizeMb { get; set; }
}
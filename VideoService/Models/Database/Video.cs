using System.ComponentModel.DataAnnotations;

namespace VideoService.Models.Database;

public class Video
{
    [Key]
    public Guid VideoId { get; set; }
    public Guid AuthorId { get; set; }
    public string Text { get; set; }
    public string? Resolution { get; set; }
    public int? DurationSeconds { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? SizeMb { get; set; }
    public required string BucketName { get; set; }
}
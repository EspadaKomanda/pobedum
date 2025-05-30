namespace VideoService.Models.Internal.Requests;

public class AddVideoRequest
{
    public Guid BuckedObjectId { get; set; }
    public Guid AuthorId { get; set; }
    public required string BucketId { get; set; }
    public string Text { get; set; }
    public string Resolution { get; set; }
}
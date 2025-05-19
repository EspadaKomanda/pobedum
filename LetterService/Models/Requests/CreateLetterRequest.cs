using LetterService.Models.Internal;

namespace LetterService.Models.Requests;

public class CreateLetterRequest
{
    public required string Title { get; set; }
    public required string Content { get; set; }
    public required string Resource { get; set; }
    public User Author { get; set; }
}
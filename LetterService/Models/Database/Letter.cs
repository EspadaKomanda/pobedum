using System.ComponentModel.DataAnnotations;

namespace LetterService.Models.Database;

public class Letter 
{
    [Key] 
    public Guid Id { get; set; }
    public bool IsLong { get; set; }
    public required string Title { get; set; }
    public string? Content { get; set; }
    public Guid AuthorId { get; set; }
    public required string Resource { get; set; }
}
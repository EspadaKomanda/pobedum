namespace LetterService.Models.DTOs;

public class LetterDTO
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Content { get; set; }
    public bool IsOwned { get; set; }
    public required string Resource { get; set; }
    public bool IsFavourite { get; set; }
    public required string Date { get; set; }
    public bool IsLong { get; set; }
}
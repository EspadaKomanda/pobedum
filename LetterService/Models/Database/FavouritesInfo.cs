using System.ComponentModel.DataAnnotations;

namespace LetterService.Models.Database;

public class FavouritesInfo
{
    [Key]
    public Guid UserId { get; set; }
    public List<Letter> Letters { get; set; }
}
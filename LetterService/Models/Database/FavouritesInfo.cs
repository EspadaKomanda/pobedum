using System.ComponentModel.DataAnnotations;

namespace LetterService.Models.Database;

public class FavouritesInfo
{
    [Key]
    public Guid UserId { get; set; }
    public ICollection<Guid> Letters { get; set; }
}
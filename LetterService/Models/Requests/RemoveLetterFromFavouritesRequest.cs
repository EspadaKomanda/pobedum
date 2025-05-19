using LetterService.Models.Internal;

namespace LetterService.Models.Requests;

public class RemoveLetterFromFavouritesRequest
{
    public User User { get; set; }
}
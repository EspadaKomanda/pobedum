using LetterService.Models.Internal;
using LetterService.Models.Requests;

namespace LetterService.Services.Favourites;

public interface IFavouritesService
{
    Task<bool> AddLetterToFavouritesAsync(AddLetterToFavouritesRequest addLetterToFavouritesRequest, Guid letterId);
    Task<bool> RemoveLetterFromFavouritesAsync(RemoveLetterFromFavouritesRequest removeLetterFromFavouritesRequest,
        Guid letterId);
    Task<bool> IsFavourite(User user, Guid letterId);
}
using LetterService.Database.Repositories;
using LetterService.Models.Internal;
using LetterService.Models.Requests;
using LetterService.Models.Database;

namespace LetterService.Services.Favourites;

public class FavouritesService : IFavouritesService
{
    #region Fields

    private readonly ILogger<IFavouritesService> _logger;
    private readonly UnitOfWork _unitOfWork;

    #endregion

    #region Constructor

    public FavouritesService(ILogger<IFavouritesService> logger, UnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    #endregion
    
    #region Methods

    public async Task<bool> AddLetterToFavouritesAsync(AddLetterToFavouritesRequest addLetterToFavouritesRequest, Guid letterId)
    {
        try
        {
            var favouriteInfo =
                await _unitOfWork.FavouritesRepository.GetByIDAsync(addLetterToFavouritesRequest.User.Id);
            if (favouriteInfo == null)
            {
                favouriteInfo = new FavouritesInfo()
                {
                    UserId = letterId,
                    Letters = _unitOfWork.LetterRepository.Get().Where(x => x.Id == letterId).ToList()
                };

                await _unitOfWork.FavouritesRepository.InsertAsync(favouriteInfo);
                return await _unitOfWork.SaveAsync();
            }

            favouriteInfo.Letters.Add(await _unitOfWork.LetterRepository.GetByIDAsync(letterId));
            _unitOfWork.FavouritesRepository.Update(favouriteInfo);
            return await _unitOfWork.SaveAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message,e);
            throw;
        }
    }

    public async Task<bool> RemoveLetterFromFavouritesAsync(RemoveLetterFromFavouritesRequest removeLetterFromFavouritesRequest,
        Guid letterId)
    {
        try
        {
            var favoritesInfo = await _unitOfWork.FavouritesRepository.GetByIDAsync(removeLetterFromFavouritesRequest.User.Id);
            favoritesInfo.Letters.Remove(await _unitOfWork.LetterRepository.GetByIDAsync(letterId));
            _unitOfWork.FavouritesRepository.Update(favoritesInfo);

            return await _unitOfWork.SaveAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message,e);
            throw;
        }
    }

    public async Task<bool> IsFavourite(User user, Guid letterId)
    {
      
        try
        {
            var favoritesInfo = await _unitOfWork.FavouritesRepository.GetByIDAsync(user.Id);
            return favoritesInfo.Letters.Any(x => x.Id ==letterId);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message,e);
            return false;
        }
    }

    #endregion
}
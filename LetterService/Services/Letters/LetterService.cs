using AutoMapper;
using LetterService.Database.Repositories;
using LetterService.Models.Database;
using LetterService.Models.DTOs;
using LetterService.Models.Internal;
using LetterService.Models.Requests;
using LetterService.Services.Favourites;
using Microsoft.EntityFrameworkCore;

namespace LetterService.Services.Letters;

public class LetterService : ILetterService
{
    #region Fields

    private readonly ILogger<ILetterService> _logger;
    private readonly UnitOfWork _unitOfWork;
    private readonly IFavouritesService _favouritesService;
    
    #endregion

    #region Constructor

    public LetterService(ILogger<ILetterService> logger, UnitOfWork unitOfWork, IFavouritesService favouritesService)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _favouritesService = favouritesService;
    }

    #endregion

    #region Methods

    public async Task<LetterDTO> CreateLetterAsync(CreateLetterRequest createLetterRequest)
    {
        try
        {
            Letter letter = new Letter()
            {
                Title = createLetterRequest.Title,
                Content = createLetterRequest.Content,
                Resource = createLetterRequest.Resource,
                AuthorId = createLetterRequest.Author.Id,
                IsLong = createLetterRequest.Content.Length > 1000
            };

            var entityEntry = await _unitOfWork.LetterRepository.InsertAsync(letter);

            if (await _unitOfWork.SaveAsync())
            {
                return new LetterDTO()
                {
                    Resource = entityEntry.Entity.Resource,
                    Title = entityEntry.Entity.Resource,
                    Content = entityEntry.Entity.Content,
                    Id = entityEntry.Entity.Id,
                    IsOwned = createLetterRequest.Author.Id == entityEntry.Entity.AuthorId,
                    IsLong = entityEntry.Entity.IsLong,
                    IsFavourite =
                        await _favouritesService.IsFavourite(createLetterRequest.Author, entityEntry.Entity.Id)
                };
            }

            throw new InvalidDataException("Wrong letter data");


        }
        catch (Exception e)
        {
            _logger.LogError(e.Message,e);
            throw;
        }
    }
    
    public async Task<LetterDTO?> GetLetterByIdAsync(User user, Guid letterId)
    {
        try
        {
            Letter letter = await _unitOfWork.LetterRepository.GetByIDAsync(letterId);
            
            return new LetterDTO()
            {
                Resource = letter.Resource,
                Title = letter.Title,
                Content = letter.Content,
                Id = letter.Id,
                IsLong = letter.IsLong,
                IsFavourite = await _favouritesService.IsFavourite(user,letterId),
                IsOwned = letter.AuthorId == user.Id
            };
           
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message,e);
            throw;
        }
    }

    public async Task<LetterDTO?> GetLetterByIdAsync(Guid letterId)
    {
        try
        {
            Letter letter = await _unitOfWork.LetterRepository.GetByIDAsync(letterId);
            
            return new LetterDTO()
            {
                Resource = letter.Resource,
                Title = letter.Title,
                Content = letter.Content,
                Id = letter.Id,
                IsLong = letter.IsLong,
                IsFavourite = false,
                IsOwned = false
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message,e);
            throw;
        }
    }

    public async Task<(List<LetterDTO> Letters, int TotalCount)> GetAllLettersAsync(User? user, int pageNumber = 1, int pageSize = 10)
    {
        var query = _unitOfWork.LetterRepository.Get();
    
        var totalItems = await query.CountAsync();
    
        var pagedLetters = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        List<LetterDTO> result = pagedLetters.Select<Letter,LetterDTO>(letter => new LetterDTO()
        {
            Resource = letter.Resource,
            Title = letter.Title,
            Content = letter.Content,
            Id = letter.Id,
            IsLong = letter.IsLong,
            IsFavourite =  _favouritesService.IsFavourite(user, letter.Id).Result,
            IsOwned = letter.AuthorId == user.Id
        }).ToList();
        
        return (result, totalItems);
       
    }

    public async Task<(List<LetterDTO> Letters, int TotalCount)> GetAllLettersAsync(int pageNumber = 1, int pageSize = 10)
    {
        var query = _unitOfWork.LetterRepository.Get();
    
        var totalItems = await query.CountAsync();
    
        var pagedLetters = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
       
        List<LetterDTO> result = pagedLetters.Select<Letter,LetterDTO>(letter => new LetterDTO()
        {
            Resource = letter.Resource,
            Title = letter.Title,
            Content = letter.Content,
            Id = letter.Id,
            IsLong = letter.IsLong,
            IsFavourite = false,
            IsOwned = false
        }).ToList();
        
        return (result, totalItems);
        
    }

    #endregion
}
using LetterService.Models.Database;
using LetterService.Models.DTOs;
using LetterService.Models.Internal;
using LetterService.Models.Requests;

namespace LetterService.Services.Letters;

public interface ILetterService
{
    Task<LetterDTO> CreateLetterAsync(CreateLetterRequest createLetterRequest);
    Task<LetterDTO?> GetLetterByIdAsync(User user, Guid letterId);
    Task<LetterDTO?> GetLetterByIdAsync( Guid letterId);
    
    Task<GetLettersRequest> GetAllLettersAsync(User user, int pageNumber = 1, int pageSize = 10);
    Task<GetLettersRequest> GetAllLettersAsync(int pageNumber = 1, int pageSize = 10);

}
using LetterService.Models.DTOs;

namespace LetterService.Models.Requests;

public class GetLettersRequest
{
    public List<LetterDTO> Letters { get; set; }
    public int TotalCount { get; set; }
}
using ApiGatewayService.Models.Microservices.LetterService.DTOs;

namespace ApiGatewayService.Models.Microservices.LetterService.Responses;

public class GetLettersRequest
{
    public List<LetterDTO> Letters { get; set; }
    public int TotalCount { get; set; }
}
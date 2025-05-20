using ApiGatewayService.Models.Microservices.Internal;

namespace ApiGatewayService.Models.Microservices.LetterService.Requests;

public class CreateLetterRequest
{
    public required string Title { get; set; }
    public required string Content { get; set; }
    public required string Resource { get; set; }
    public required string Date { get; set; }
    public User Author { get; set; }
}
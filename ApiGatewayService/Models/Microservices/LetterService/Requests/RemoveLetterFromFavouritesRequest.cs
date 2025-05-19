using ApiGatewayService.Models.Microservices.Internal;

namespace ApiGatewayService.Models.Microservices.LetterService.Requests;

public class RemoveLetterFromFavouritesRequest
{
    public User User { get; set; }
}
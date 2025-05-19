using ApiGatewayService.Models.Microservices.Internal;

namespace ApiGatewayService.Models.Microservices.LetterService.Requests;

public class AddLetterToFavouritesRequest
{
    public User User { get; set; } 
}
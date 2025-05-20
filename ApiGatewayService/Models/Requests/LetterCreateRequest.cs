namespace ApiGatewayService.Models.Requests;

public class LetterCreateRequest
{
    public string Title { get; set; }
    public string Content { get; set; }
    public string Date { get; set; }
    public string Resource { get; set; }
}
using VideoService.Models.DTOs;

namespace VideoService.Models;

public class GetVideosByUserIdResponse
{
    public List<VideoDTO>  content { get; set; }
    public PageInfo page { get; set; }
}


public class PageInfo
{
    public int size { get; set; }
    public int number { get; set; }
    public int totalElements { get; set; }
    public int totalPages { get; set; }
}
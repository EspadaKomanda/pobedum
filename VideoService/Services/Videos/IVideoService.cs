using VideoService.Models.DTOs;
using VideoService.Models.Internal;
using VideoService.Models.Internal.Requests;

namespace VideoService.Services.Videos;

public interface IVideoService
{
    Task<VideoDTO> GetVideoByIdAsync(Guid videoId);
    Task<(List<VideoDTO> Videos, int TotalCount)> GetVideosByUserId(User user, int page, int size);
    Task<List<VideoDTO>> GetVideosByLetterId(Guid letterId);
    Task<bool> AddVideo(AddVideoRequest addVideoRequest);
}
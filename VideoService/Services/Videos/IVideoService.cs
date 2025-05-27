using VideoService.Models;
using VideoService.Models.DTOs;
using VideoService.Models.Internal;
using VideoService.Models.Internal.Requests;

namespace VideoService.Services.Videos;

public interface IVideoService
{
    Task<VideoDTO> GetVideoByIdAsync(Guid videoId);
    Task<GetVideosByUserIdResponse> GetVideosByUserId(User user, int page, int size);
    Task<List<VideoDTO>> GetVideosByLetterId(Guid letterId);
    Task<bool> AddVideo(AddVideoRequest addVideoRequest);
}
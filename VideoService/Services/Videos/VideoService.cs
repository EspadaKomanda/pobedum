using Microsoft.EntityFrameworkCore;
using VideoService.Database.Repositories;
using VideoService.Models;
using VideoService.Models.Database;
using VideoService.Models.DTOs;
using VideoService.Models.Internal;
using VideoService.Models.Internal.Requests;
using VideoService.Services.Utils.S3Storage;

namespace VideoService.Services.Videos;

public class VideoService : IVideoService
{
    #region Fields

    private readonly ILogger<IVideoService> _logger;
    private readonly IS3StorageService _s3StorageService;
    private readonly UnitOfWork _unitOfWork;

    #endregion

    #region Constructor

    public VideoService(ILogger<IVideoService> logger, IS3StorageService s3StorageService, UnitOfWork unitOfWork)
    {
        _logger = logger;
        _s3StorageService = s3StorageService;
        _unitOfWork = unitOfWork;
    }

    #endregion
    
    #region Methods

    public async Task<VideoDTO> GetVideoByIdAsync(Guid videoId)
    {
        try
        {
            var videoInformation = await _unitOfWork.VideoRepository.GetByIDAsync(videoId);
            if (await _s3StorageService.CheckIfBucketExists(videoInformation.BucketName))
            {
                var videoUrl = await _s3StorageService.GetVideoUrlFromS3Bucket(videoInformation.VideoId,
                    videoInformation.BucketName);
                return new VideoDTO()
                {
                    VideoId = videoInformation.VideoId,
                    CreatedAt = videoInformation.CreatedAt,
                    DurationSeconds = videoInformation.DurationSeconds,
                    LetterId = videoInformation.LetterId,
                    Resolution = videoInformation.Resolution,
                    SizeMb = videoInformation.SizeMb,
                    VideoUrl = videoUrl
                };
            }

            throw new DirectoryNotFoundException("Bucket not found!");
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message,e);
            throw;
        }
    }

    public async Task<GetVideosByUserIdResponse> GetVideosByUserId(User user, int page, int size)
    {
        
        var query = _unitOfWork.VideoRepository.Get();
    
        var totalItems = await query.CountAsync();
    
        var pagedVideos = await query
            .Skip(page * size)
            .Take(size)
            .ToListAsync();
       
        List<VideoDTO> result = pagedVideos.Select<Video,VideoDTO>(videoInformation => new VideoDTO()
        {
            VideoId = videoInformation.VideoId,
            CreatedAt = videoInformation.CreatedAt,
            DurationSeconds = videoInformation.DurationSeconds,
            LetterId = videoInformation.LetterId,
            Resolution = videoInformation.Resolution,
            SizeMb = videoInformation.SizeMb,
            VideoUrl =  _s3StorageService.GetVideoUrlFromS3Bucket(videoInformation.VideoId,
                videoInformation.BucketName).Result
        }).ToList();
        
        return new GetVideosByUserIdResponse()
        {
            content = result,
            page = new PageInfo()
            {
                size = size,
                number = page,
                totalElements = result.Count,
                totalPages = (int)Math.Ceiling(Convert.ToDouble(totalItems) / size)
            }
        };
    }

    public async Task<List<VideoDTO>> GetVideosByLetterId(Guid letterId)
    {
        try
        {
            List<VideoDTO> result = await _unitOfWork.VideoRepository.Get().Select<Video,VideoDTO>(videoInformation => new VideoDTO()
            {
                VideoId = videoInformation.VideoId,
                CreatedAt = videoInformation.CreatedAt,
                DurationSeconds = videoInformation.DurationSeconds,
                LetterId = videoInformation.LetterId,
                Resolution = videoInformation.Resolution,
                SizeMb = videoInformation.SizeMb,
                VideoUrl =  _s3StorageService.GetVideoUrlFromS3Bucket(videoInformation.VideoId,
                    videoInformation.BucketName).Result
            }).ToListAsync();

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message,e);
            throw;
        }
       
        
    }

    public async Task<bool> AddVideo(AddVideoRequest addVideoRequest)
    {
        try
        {
            await _unitOfWork.VideoRepository.InsertAsync(new Video()
            {
                BucketName = addVideoRequest.BucketId,
                AuthorId = addVideoRequest.AuthorId,
                LetterId = addVideoRequest.LetterId
            });
            return await _unitOfWork.SaveAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message,e);
            throw;
        }
    }

    #endregion
}
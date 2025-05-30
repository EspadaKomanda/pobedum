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
            var videoInformation =  _unitOfWork.VideoRepository.Get().FirstOrDefault(x=> x.VideoId == videoId);
            if (await _s3StorageService.CheckIfBucketExists(videoInformation.BucketName))
            {
                var videoUrl = await _s3StorageService.GetVideoUrlFromS3Bucket(videoInformation.VideoId,
                    videoInformation.BucketName);
                return new VideoDTO()
                {
                    VideoId = videoInformation.VideoId,
                    CreatedAt = videoInformation.CreatedAt,
                    DurationSeconds = videoInformation.DurationSeconds,
                    Text = videoInformation.Text,
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
        
        var query = _unitOfWork.VideoRepository.Get().Where(x => x.AuthorId == user.Id);
    
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
            Text = videoInformation.Text,
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
                Text = videoInformation.Text,
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


            string url = await _s3StorageService.GetVideoUrlFromS3Bucket(addVideoRequest.BuckedObjectId,
                addVideoRequest.BucketId);
            var info = await DownloadVideoAsync(url);
            Random random = new Random();
            await _unitOfWork.VideoRepository.InsertAsync(new Video()
            {
                VideoId = addVideoRequest.BuckedObjectId,
                BucketName = addVideoRequest.BucketId,
                AuthorId = addVideoRequest.AuthorId,
                Resolution = addVideoRequest.Resolution,
                CreatedAt = DateTime.UtcNow,
                Text = addVideoRequest.Text,
                DurationSeconds = (int)Math.Ceiling(info),
                SizeMb = random.Next(30,100)
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

    #region CringeUtils

    public async Task<double> DownloadVideoAsync(string videoUrl)
    {
        // Download the video
        HttpClient httpClient = new HttpClient();
        var response = await httpClient.GetAsync(videoUrl);
        response.EnsureSuccessStatusCode();

        // Read the video content into a MemoryStream
        await using var memoryStream = new MemoryStream();
        await response.Content.CopyToAsync(memoryStream);

        // Get the size of the video in megabytes
        double sizeInMegabytes = memoryStream.Length / (1024.0 * 1024.0); // Convert bytes to megabytes

        // Duration cannot be calculated without a library, so we return 0 for duration
        return sizeInMegabytes;
    }

    #endregion
}
using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;

namespace VideoService.Services.Utils.S3Storage;

public class S3StorageService : IS3StorageService
{
    #region Fields

    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<S3StorageService> _logger;

    #endregion

    #region Constructor
    
    public S3StorageService(IAmazonS3 s3Client, ILogger<S3StorageService> logger)
    {
        _s3Client = s3Client;
        _logger = logger;
    }

    #endregion

    #region Methods

    public async Task<string> GetVideoUrlFromS3Bucket(Guid videoKey, string bucketName)
    {
        try
        {
            GetObjectMetadataResponse metadataResponse = await _s3Client.GetObjectMetadataAsync(bucketName,videoKey.ToString()+".mp4");
            
            if(metadataResponse.HttpStatusCode == HttpStatusCode.OK)
            {
                var response =  _s3Client.GetPreSignedURL(new GetPreSignedUrlRequest(){ BucketName = bucketName, Key =  videoKey.ToString()+".mp4", Expires = DateTime.Now.AddYears(10), Protocol = Protocol.HTTPS});
                
                _logger.LogInformation($"Uri for video {videoKey} aquired from S3 bucket {bucketName}!");
                return response;
            }
            if(metadataResponse.HttpStatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogError($"Video {videoKey} not found in S3 bucket {bucketName}!");
                throw new Exception($"Video {videoKey} not found in S3 bucket {bucketName}!");
            }
            if(metadataResponse.HttpStatusCode == HttpStatusCode.InternalServerError)
            {
                _logger.LogError($"Failed to get video {videoKey} from S3 bucket {bucketName}, storage unavailible!");
                throw new Exception($"Failed to get video {videoKey} from S3 bucket {bucketName}, storage unavailible!");
            }
            _logger.LogError($"Failed to get video {videoKey} from S3 bucket {bucketName}, unhandled exception!" + metadataResponse.ToString());
            throw new Exception($"Failed to get video {videoKey} from S3 bucket {bucketName}!, unhandled exception!" + metadataResponse.ToString());
                                
        }      
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Video from S3 bucket!");
            throw;
        }
    }

    public async Task<bool> CheckIfBucketExists(string bucketName)
    {
        return await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);
    }

    #endregion
}
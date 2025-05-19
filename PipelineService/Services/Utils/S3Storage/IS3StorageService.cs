namespace PipelineService.Services.Utils.S3Storage;

public interface IS3StorageService
{
    Task<string> GetVideoUrlFromS3Bucket(Guid videoKey, string bucketName);
    Task<bool> CheckIfBucketExists(string bucketName);
}
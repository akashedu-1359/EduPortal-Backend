using Amazon.S3;
using Amazon.S3.Model;
using EduPortal.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EduPortal.Infrastructure.Services;

public class CloudflareR2StorageService : IStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucket;
    private readonly ILogger<CloudflareR2StorageService> _logger;

    public CloudflareR2StorageService(IAmazonS3 s3, IConfiguration config, ILogger<CloudflareR2StorageService> logger)
    {
        _s3 = s3;
        _bucket = config["Storage:BucketName"] ?? throw new InvalidOperationException("Storage:BucketName not configured");
        _logger = logger;
    }

    public Task<string> GetUploadUrlAsync(string objectKey, string contentType, int expirySeconds = 3600, CancellationToken ct = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucket,
            Key = objectKey,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.AddSeconds(expirySeconds),
            ContentType = contentType
        };
        return Task.FromResult(_s3.GetPreSignedURL(request));
    }

    public Task<string> GetReadUrlAsync(string objectKey, int expirySeconds = 3600, CancellationToken ct = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucket,
            Key = objectKey,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.AddSeconds(expirySeconds)
        };
        return Task.FromResult(_s3.GetPreSignedURL(request));
    }

    public async Task UploadAsync(string objectKey, Stream content, string contentType, CancellationToken ct = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = _bucket,
            Key = objectKey,
            InputStream = content,
            ContentType = contentType
        };
        await _s3.PutObjectAsync(request, ct);
    }

    public async Task DeleteAsync(string objectKey, CancellationToken ct = default)
    {
        await _s3.DeleteObjectAsync(_bucket, objectKey, ct);
    }
}

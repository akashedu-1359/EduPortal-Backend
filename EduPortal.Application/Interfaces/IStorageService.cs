namespace EduPortal.Application.Interfaces;

public interface IStorageService
{
    /// <summary>Returns a pre-signed URL for direct client upload.</summary>
    Task<string> GetUploadUrlAsync(string objectKey, string contentType, int expirySeconds = 3600, CancellationToken ct = default);

    /// <summary>Returns a pre-signed URL for reading a private object.</summary>
    Task<string> GetReadUrlAsync(string objectKey, int expirySeconds = 3600, CancellationToken ct = default);

    /// <summary>Uploads a file from a stream (used by server-side generators like QuestPDF).</summary>
    Task UploadAsync(string objectKey, Stream content, string contentType, CancellationToken ct = default);

    Task DeleteAsync(string objectKey, CancellationToken ct = default);
}

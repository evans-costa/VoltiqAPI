namespace Voltiq.Application.Common.Interfaces.Storage;

public interface IStorageService
{
    Task<string> UploadAsync(string fileName, byte[] data, string contentType, CancellationToken cancellationToken = default);
    Task<string> GetSasUrlAsync(string fileName, int expirationInHours = 1, CancellationToken cancellationToken = default);
}

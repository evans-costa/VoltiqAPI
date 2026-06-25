using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;
using Voltiq.Application.Common.Interfaces.Storage;

namespace Voltiq.Infrastructure.Storage;

public class AzureBlobStorageService : IStorageService
{
    private const string CONTAINER_NAME = "reports";
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("StorageAccount") ??
                               configuration["AzureWebJobsStorage"] ?? "UseDevelopmentStorage=true";
        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task<string> UploadAsync(string fileName, byte[] data, string contentType,
        CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(CONTAINER_NAME);
        await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var blobClient = containerClient.GetBlobClient(fileName);

        using var stream = new MemoryStream(data);
        await blobClient.UploadAsync(stream, true, cancellationToken);

        return blobClient.Uri.ToString();
    }

    public async Task<string> GetSasUrlAsync(string fileName, int expirationInHours = 1,
        CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(CONTAINER_NAME);
        var blobClient = containerClient.GetBlobClient(fileName);

        if (!await blobClient.ExistsAsync(cancellationToken))
            return string.Empty;

        if (blobClient.CanGenerateSasUri)
        {
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = CONTAINER_NAME,
                BlobName = fileName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(expirationInHours)
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);

            var sasUri = blobClient.GenerateSasUri(sasBuilder);
            return sasUri.ToString();
        }

        return blobClient.Uri.ToString();
    }
}

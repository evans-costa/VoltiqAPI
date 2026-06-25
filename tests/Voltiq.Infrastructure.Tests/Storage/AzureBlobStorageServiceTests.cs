using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Shouldly;
using Voltiq.CommonTestUtilities.Fixtures;
using Voltiq.Infrastructure.Storage;

namespace Voltiq.Infrastructure.Tests.Storage;

public class AzureBlobStorageServiceTests(AzuriteContainerFixture fixture)
    : IClassFixture<AzuriteContainerFixture>, IAsyncLifetime
{
    private AzureBlobStorageService _storageService = null!;
    private BlobServiceClient _blobServiceClient = null!;

    public ValueTask InitializeAsync()
    {
        var connectionString = fixture.Container.GetConnectionString();
        _blobServiceClient = new BlobServiceClient(connectionString);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:StorageAccount", connectionString }
            })
            .Build();

        _storageService = new AzureBlobStorageService(configuration);
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task UploadAsync_ShouldUploadFileAndReturnBlobUri()
    {
        // Arrange
        var fileName = $"test-{Guid.NewGuid()}.txt";
        var fileData = "Hello Azurite!"u8.ToArray();
        var contentType = "text/plain";

        // Act
        var resultUri = await _storageService.UploadAsync(fileName, fileData, contentType, CancellationToken.None);

        // Assert
        resultUri.ShouldNotBeNullOrEmpty();
        resultUri.ShouldContain(fileName);
        resultUri.ShouldContain("reports");

        // Verify the blob actually exists in the container
        var containerClient = _blobServiceClient.GetBlobContainerClient("reports");
        var blobClient = containerClient.GetBlobClient(fileName);
        var exists = await blobClient.ExistsAsync(TestContext.Current.CancellationToken);
        exists.Value.ShouldBeTrue();
    }

    [Fact]
    public async Task GetSasUrlAsync_WhenBlobExists_ShouldReturnSasUri()
    {
        // Arrange
        var fileName = $"test-sas-{Guid.NewGuid()}.txt";
        var fileData = "SAS Test Data"u8.ToArray();
        var contentType = "text/plain";

        // Upload first
        await _storageService.UploadAsync(fileName, fileData, contentType, CancellationToken.None);

        // Act
        var sasUrl = await _storageService.GetSasUrlAsync(fileName, 1, CancellationToken.None);

        // Assert
        sasUrl.ShouldNotBeNullOrEmpty();
        sasUrl.ShouldContain(fileName);
        sasUrl.ShouldContain("sig="); // SAS token signature query parameter
    }

    [Fact]
    public async Task GetSasUrlAsync_WhenBlobDoesNotExist_ShouldReturnEmptyString()
    {
        // Arrange
        var fileName = $"non-existent-{Guid.NewGuid()}.txt";

        // Act
        var sasUrl = await _storageService.GetSasUrlAsync(fileName, 1, CancellationToken.None);

        // Assert
        sasUrl.ShouldBeEmpty();
    }
}

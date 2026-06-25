using System.Text.Json;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Shouldly;
using Voltiq.CommonTestUtilities.Fixtures;
using Voltiq.Infrastructure.Queue;

namespace Voltiq.Infrastructure.Tests.Queue;

public class AzureQueueServiceTests(AzuriteContainerFixture fixture)
    : IClassFixture<AzuriteContainerFixture>, IAsyncLifetime
{
    private AzureQueueService _queueService = null!;
    private QueueServiceClient _queueServiceClient = null!;

    public ValueTask InitializeAsync()
    {
        var connectionString = fixture.Container.GetConnectionString();
        _queueServiceClient = new QueueServiceClient(connectionString);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:StorageAccount", connectionString }
            })
            .Build();

        _queueService = new AzureQueueService(configuration);
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task SendMessageAsync_ShouldCreateQueueAndSendMessage()
    {
        // Arrange
        var queueName = $"test-queue-{Guid.NewGuid()}";
        var testMessage = new TestMessagePayload { MessageId = Guid.NewGuid(), Content = "Hello Queue!" };

        // Act
        await _queueService.SendMessageAsync(queueName, testMessage, CancellationToken.None);

        // Assert
        var queueClient = _queueServiceClient.GetQueueClient(queueName);
        var exists = await queueClient.ExistsAsync(TestContext.Current.CancellationToken);
        exists.Value.ShouldBeTrue();

        var messages = await queueClient.ReceiveMessagesAsync(1, cancellationToken: TestContext.Current.CancellationToken);
        messages.Value.Length.ShouldBe(1);

        var receivedMessage = messages.Value[0];
        var payload = JsonSerializer.Deserialize<TestMessagePayload>(receivedMessage.Body.ToString());

        payload.ShouldNotBeNull();
        payload.MessageId.ShouldBe(testMessage.MessageId);
        payload.Content.ShouldBe(testMessage.Content);
    }

    private class TestMessagePayload
    {
        public Guid MessageId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}

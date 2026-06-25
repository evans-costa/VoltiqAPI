using System.Text.Json;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Voltiq.Application.Common.Interfaces.Queue;

namespace Voltiq.Infrastructure.Queue;

public class AzureQueueService : IQueueService
{
    private readonly QueueServiceClient _queueServiceClient;

    public AzureQueueService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("StorageAccount") ??
                               configuration["AzureWebJobsStorage"] ?? "UseDevelopmentStorage=true";
        _queueServiceClient = new QueueServiceClient(connectionString);
    }

    public async Task SendMessageAsync<T>(string queueName, T message,
        CancellationToken cancellationToken = default)
    {
        var queueClient = _queueServiceClient.GetQueueClient(queueName);
        await queueClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var messageBody = JsonSerializer.Serialize(message);
        await queueClient.SendMessageAsync(messageBody, cancellationToken);
    }
}

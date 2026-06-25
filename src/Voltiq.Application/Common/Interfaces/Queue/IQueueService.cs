namespace Voltiq.Application.Common.Interfaces.Queue;

public interface IQueueService
{
    Task SendMessageAsync<T>(string queueName, T message, CancellationToken cancellationToken = default);
}

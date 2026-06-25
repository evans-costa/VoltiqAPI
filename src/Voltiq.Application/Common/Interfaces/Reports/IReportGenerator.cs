namespace Voltiq.Application.Common.Interfaces.Reports;

public interface IReportGenerator
{
    Task<byte[]> GenerateAsync<TData>(TData data, CancellationToken cancellationToken = default);
}

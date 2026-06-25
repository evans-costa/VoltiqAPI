namespace Voltiq.Application.Common.Interfaces.Reports;

public interface IReportStrategy<in TData>
{
    Task<byte[]> GenerateAsync(TData data, CancellationToken cancellationToken = default);
}

using Microsoft.Extensions.DependencyInjection;
using QuestPDF;
using QuestPDF.Infrastructure;
using Voltiq.Application.Common.Interfaces.Reports;

namespace Voltiq.Infrastructure.Reports;

public class QuestPdfReportGenerator : IReportGenerator
{
    private readonly IServiceProvider _serviceProvider;

    public QuestPdfReportGenerator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateAsync<TData>(TData data,
        CancellationToken cancellationToken = default)
    {
        var strategy = _serviceProvider.GetRequiredService<IReportStrategy<TData>>();
        return await strategy.GenerateAsync(data, cancellationToken);
    }
}

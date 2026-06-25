using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Voltiq.Application.Common.Interfaces.Reports;

namespace Voltiq.Infrastructure.Reports.Strategies;

public class BudgetReportStrategy : IReportStrategy<BudgetReportData>
{
    public Task<byte[]> GenerateAsync(BudgetReportData data, CancellationToken cancellationToken = default)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12));

                page.Header()
                    .Text($"Orçamento: {data.ProjectName}")
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(x =>
                    {
                        x.Spacing(20);
                        x.Item().Text($"Cliente: {data.ClientName}");
                        x.Item().Text($"Data: {data.CreatedAt:dd/MM/yyyy}");
                        x.Item().Text($"Valor Total: {data.TotalAmount:C}");
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
            });
        });

        var pdfBytes = document.GeneratePdf();
        return Task.FromResult(pdfBytes);
    }
}

using QuestPDF.Infrastructure;
using Shouldly;
using Voltiq.Application.Common.Interfaces.Reports;
using Voltiq.Infrastructure.Reports.Strategies;

namespace Voltiq.Infrastructure.Tests.Reports;

public class BudgetReportStrategyTests
{
    public BudgetReportStrategyTests()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    [Fact]
    public async Task GenerateAsync_ShouldReturnPdfBytes_WhenGivenValidData()
    {
        // Arrange
        var strategy = new BudgetReportStrategy();
        var data = new BudgetReportData
        {
            BudgetId = Guid.NewGuid(),
            ProjectName = "Instalação Residencial",
            ClientName = "José da Silva",
            CreatedAt = new DateTime(2026, 6, 25),
            TotalAmount = 1500.50m
        };

        // Act
        var pdfBytes = await strategy.GenerateAsync(data, CancellationToken.None);

        // Assert
        pdfBytes.ShouldNotBeNull();
        pdfBytes.Length.ShouldBeGreaterThan(0);
    }
}

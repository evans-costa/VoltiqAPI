using Moq;
using Shouldly;
using Voltiq.Application.Common.Interfaces.Reports;
using Voltiq.Infrastructure.Reports;

namespace Voltiq.Infrastructure.Tests.Reports;

public class QuestPdfReportGeneratorTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock = new();
    private readonly Mock<IReportStrategy<BudgetReportData>> _reportStrategyMock = new();

    [Fact]
    public async Task GenerateAsync_ShouldCallStrategyAndReturnBytes()
    {
        // Arrange
        var expectedBytes = "PDF-CONTENT"u8.ToArray();
        var data = new BudgetReportData { ProjectName = "Test" };

        _reportStrategyMock.Setup(s => s.GenerateAsync(data, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBytes);

        _serviceProviderMock.Setup(sp => sp.GetService(typeof(IReportStrategy<BudgetReportData>)))
            .Returns(_reportStrategyMock.Object);

        var generator = new QuestPdfReportGenerator(_serviceProviderMock.Object);

        // Act
        var result = await generator.GenerateAsync(data, CancellationToken.None);

        // Assert
        result.ShouldBe(expectedBytes);
        _reportStrategyMock.Verify(s => s.GenerateAsync(data, It.IsAny<CancellationToken>()), Times.Once);
    }
}

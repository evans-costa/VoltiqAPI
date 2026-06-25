namespace Voltiq.Application.Common.Interfaces.Reports;

public class BudgetReportData
{
    public Guid BudgetId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}

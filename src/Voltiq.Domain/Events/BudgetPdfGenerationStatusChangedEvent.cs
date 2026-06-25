using Voltiq.Domain.Enums;

namespace Voltiq.Domain.Events;

public sealed record BudgetPdfGenerationStatusChangedEvent(Guid BudgetId, PdfGenerationStatus Status) : BaseDomainEvent;

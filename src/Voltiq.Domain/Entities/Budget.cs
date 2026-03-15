using Voltiq.Domain.Enums;
using Voltiq.Domain.Events;
using Voltiq.Exceptions.Exceptions;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Domain.Entities;

public sealed class Budget : AuditableEntity
{
    public Guid ClientId { get; private set; }
    public decimal TotalAmount { get; private set; }
    public BudgetStatus Status { get; private set; }
    public string? PdfUrl { get; private set; }

    private readonly List<BudgetItem> _items = [];
    public IReadOnlyCollection<BudgetItem> Items => _items.AsReadOnly();

    private Budget() { }

    private Budget(Guid clientId)
    {
        ClientId = clientId;
        TotalAmount = 0m;
        Status = BudgetStatus.Draft;
        AddDomainEvent(new BudgetRegisteredEvent(Id));
    }

    public static Budget Register(Guid clientId)
    {
        if (clientId == Guid.Empty)
            throw new DomainException(ResourceErrorMessages.ORCAMENTO_CLIENTE_OBRIGATORIO);

        return new Budget(clientId);
    }

    public void AddItem(BudgetItem item)
    {
        _items.Add(item);
        RecalculateTotals();
    }

    public void RecalculateTotals()
    {
        TotalAmount = _items.Sum(i => i.TotalPrice);
    }
}

using Voltiq.Domain.Enums;
using Voltiq.Domain.Events;
using Voltiq.Exceptions.Exceptions;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Domain.Entities;

public sealed class Budget : AuditableEntity
{
    public Guid UserId { get; private set; }
    public Guid ClientId { get; private set; }
    public decimal TotalAmount { get; private set; }
    public BudgetStatus Status { get; private set; }
    public string? PdfUrl { get; private set; }

    private readonly List<BudgetItem> _items = [];
    public IReadOnlyCollection<BudgetItem> Items => _items.AsReadOnly();

    private Budget() { }

    private Budget(Guid userId, Guid clientId)
    {
        UserId = userId;
        ClientId = clientId;
        TotalAmount = 0m;
        Status = BudgetStatus.Draft;
        AddDomainEvent(new BudgetRegisteredEvent(Id));
    }

    public static Budget Register(Guid userId, Guid clientId)
    {
        if (userId == Guid.Empty)
            throw new DomainException(ResourceErrorMessages.ORCAMENTO_USUARIO_OBRIGATORIO);

        if (clientId == Guid.Empty)
            throw new DomainException(ResourceErrorMessages.ORCAMENTO_CLIENTE_OBRIGATORIO);

        return new Budget(userId, clientId);
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

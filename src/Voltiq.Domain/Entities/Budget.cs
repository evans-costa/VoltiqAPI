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
    public PdfGenerationStatus? PdfGenerationStatus { get; private set; }

    private readonly List<BudgetItem> _items = [];
    public IReadOnlyCollection<BudgetItem> Items => _items.AsReadOnly();

    public Client Client { get; private set; } = null!;

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

    public void Edit(Guid clientId, IReadOnlyCollection<BudgetItem> items)
    {
        if (Status != BudgetStatus.Draft)
            throw new DomainException(ResourceErrorMessages.ORCAMENTO_APENAS_RASCUNHO_PODE_SER_EDITADO);

        if (clientId == Guid.Empty)
            throw new DomainException(ResourceErrorMessages.ORCAMENTO_CLIENTE_OBRIGATORIO);

        if (items is null || items.Count == 0)
            throw new DomainException(ResourceErrorMessages.ORCAMENTO_ITEMS_OBRIGATORIOS);

        ClientId = clientId;

        _items.Clear();
        foreach (var item in items)
        {
            _items.Add(item);
        }

        RecalculateTotals();
    }

    public void RecalculateTotals()
    {
        TotalAmount = _items.Sum(i => i.TotalPrice);
    }

    public void FinalizeBudget()
    {
        if (Status != BudgetStatus.Draft)
            throw new DomainException(ResourceErrorMessages.ORCAMENTO_APENAS_RASCUNHO_PODE_SER_FINALIZADO);

        if (_items.Count == 0)
            throw new DomainException(ResourceErrorMessages.ORCAMENTO_ITEMS_OBRIGATORIOS);

        Status = BudgetStatus.Finalized;
        PdfGenerationStatus = Enums.PdfGenerationStatus.Pending;
        AddDomainEvent(new BudgetFinalizedEvent(Id));
    }

    public void Approve()
    {
        if (Status != BudgetStatus.Finalized)
            throw new DomainException(ResourceErrorMessages.ORCAMENTO_STATUS_INVALIDO_PARA_APROVACAO);

        if (PdfGenerationStatus != Enums.PdfGenerationStatus.Success)
            throw new DomainException(ResourceErrorMessages.ORCAMENTO_PDF_NAO_DISPONIVEL);

        Status = BudgetStatus.Approved;
        AddDomainEvent(new BudgetApprovedEvent(Id));
    }

    public void Reject()
    {
        if (Status != BudgetStatus.Finalized)
            throw new DomainException(ResourceErrorMessages.ORCAMENTO_STATUS_INVALIDO_PARA_REJEICAO);

        if (PdfGenerationStatus != Enums.PdfGenerationStatus.Success)
            throw new DomainException(ResourceErrorMessages.ORCAMENTO_PDF_NAO_DISPONIVEL);

        Status = BudgetStatus.Rejected;
        AddDomainEvent(new BudgetRejectedEvent(Id));
    }

    public void StartPdfProcessing()
    {
        if (Status != BudgetStatus.Finalized)
            throw new DomainException(ResourceErrorMessages.ORCAMENTO_STATUS_INVALIDO_PARA_GERAR_PDF);

        PdfGenerationStatus = Enums.PdfGenerationStatus.Processing;
        AddDomainEvent(new BudgetPdfGenerationStatusChangedEvent(Id, Enums.PdfGenerationStatus.Processing));
    }

    public void SetPdfGenerationSuccess(string pdfUrl)
    {
        if (Status != BudgetStatus.Finalized)
            throw new DomainException(ResourceErrorMessages.ORCAMENTO_STATUS_INVALIDO_PARA_GERAR_PDF);

        if (string.IsNullOrWhiteSpace(pdfUrl))
            throw new DomainException(ResourceErrorMessages.ORCAMENTO_PDF_URL_OBRIGATORIA);

        PdfUrl = pdfUrl;
        PdfGenerationStatus = Enums.PdfGenerationStatus.Success;
        AddDomainEvent(new BudgetPdfGenerationStatusChangedEvent(Id, Enums.PdfGenerationStatus.Success));
    }

    public void SetPdfGenerationFailed()
    {
        if (Status != BudgetStatus.Finalized)
            throw new DomainException(ResourceErrorMessages.ORCAMENTO_STATUS_INVALIDO_PARA_GERAR_PDF);

        PdfGenerationStatus = Enums.PdfGenerationStatus.Failed;
        AddDomainEvent(new BudgetPdfGenerationStatusChangedEvent(Id, Enums.PdfGenerationStatus.Failed));
    }
}

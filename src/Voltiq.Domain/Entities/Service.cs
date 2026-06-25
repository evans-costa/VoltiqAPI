using Voltiq.Domain.Events;
using Voltiq.Exceptions.Exceptions;
using Voltiq.Exceptions.Resources;

namespace Voltiq.Domain.Entities;

public sealed class Service : AuditableEntity
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = null!;
    public decimal BasePrice { get; private set; }
    public bool IsActive { get; private set; }

    private Service() { }

    public static Service Register(Guid userId, string name, decimal basePrice)
    {
        throw new NotImplementedException();
    }

    public void Deactivate()
    {
        throw new NotImplementedException();
    }

    public void Activate()
    {
        throw new NotImplementedException();
    }

    public void Update(string name, decimal basePrice)
    {
        throw new NotImplementedException();
    }
}

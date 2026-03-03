namespace Voltiq.Exceptions.Exceptions;

public class NotFoundException : DomainException
{
    public string EntityName { get; }
    public object Key { get; }

    public NotFoundException(string entityName, object key)
        : base(string.Format(
            Resources.ResourceErrorMessages.ENTIDADE_NAO_ENCONTRADA,
            entityName,
            key))
    {
        EntityName = entityName;
        Key = key;
    }
}

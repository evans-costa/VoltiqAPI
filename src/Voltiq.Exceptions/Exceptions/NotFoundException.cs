namespace Voltiq.Exceptions.Exceptions;

public class NotFoundException : DomainException
{
    public string EntityName { get; }
    public object Key { get; }

    public NotFoundException(string entityName, object key)
        : base(string.Format(
            Resources.ResourceErrorMessages.Entidade_NaoEncontrada,
            entityName,
            key))
    {
        EntityName = entityName;
        Key = key;
    }
}

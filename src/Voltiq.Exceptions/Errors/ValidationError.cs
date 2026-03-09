
namespace Voltiq.Exceptions.Errors;

public sealed class ValidationError(string propertyName, string message) : Error(message)
{
    public string PropertyName { get; } = propertyName;
}

namespace Voltiq.Exceptions.Errors;

public sealed class UnauthorizedError(string message) : Error(message);

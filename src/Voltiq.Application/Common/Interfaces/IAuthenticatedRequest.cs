using MediatR;

namespace Voltiq.Application.Common.Interfaces;

public interface IAuthenticatedRequest<TResponse> : IRequest<TResponse>
{
    Guid UserId { get; set; }
}

using MediatR;

namespace Voltiq.Application.Common.Interfaces;

public interface IAuthenticatedRequest<out TResponse> : IRequest<TResponse>
{
    Guid UserId { get; set; }
}

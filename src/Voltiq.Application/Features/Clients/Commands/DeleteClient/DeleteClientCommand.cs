using ErrorOr;
using Voltiq.Application.Common.Interfaces;

namespace Voltiq.Application.Features.Clients.Commands.DeleteClient;

public sealed record DeleteClientCommand(Guid Id) : IAuthenticatedRequest<ErrorOr<Deleted>>
{
    public Guid UserId { get; set; }
}

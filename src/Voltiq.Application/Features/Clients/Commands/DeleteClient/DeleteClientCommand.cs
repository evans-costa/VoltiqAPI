using ErrorOr;
using MediatR;

namespace Voltiq.Application.Features.Clients.Commands.DeleteClient;

public sealed record DeleteClientCommand(Guid Id) : IRequest<ErrorOr<Deleted>>;

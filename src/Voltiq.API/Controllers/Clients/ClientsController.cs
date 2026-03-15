using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Voltiq.Application.Features.Clients;
using Voltiq.Application.Features.Clients.Commands.RegisterClient;
using Voltiq.Application.Features.Clients.Commands.DeleteClient;
using Voltiq.Application.Features.Clients.Commands.UpdateClient;
using Voltiq.Application.Features.Clients.Queries.GetClientById;
using Voltiq.Application.Features.Clients.Queries.GetClients;
using Voltiq.Application.Mappings.Clients;

namespace Voltiq.API.Controllers.Clients;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/clients")]
public sealed class ClientsController : BaseApiController
{
    /// <summary>Registers a new client for the authenticated user.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClientResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterClientRequest request,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(request.ToCommand(), cancellationToken);

        return result.Match(
            client => CreatedAtAction(nameof(GetById), new { id = client.Id }, client),
            ToErrorResult);
    }

    /// <summary>Returns all clients belonging to the authenticated user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ClientResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetClientsQuery(), cancellationToken);

        return result.Match(Ok, ToErrorResult);
    }

    /// <summary>Returns a specific client by ID (must belong to the authenticated user).</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ClientResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetClientByIdQuery(id), cancellationToken);

        return result.Match(Ok, ToErrorResult);
    }

    /// <summary>Updates a client (must belong to the authenticated user).</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateClientRequest request,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(request.ToCommand(id), cancellationToken);

        return result.Match(_ => NoContent(), ToErrorResult);
    }

    /// <summary>Deletes a client (must belong to the authenticated user).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new DeleteClientCommand(id), cancellationToken);

        return result.Match(_ => NoContent(), ToErrorResult);
    }
}

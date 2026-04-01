using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Voltiq.Application.Features.Materials;
using Voltiq.Application.Features.Materials.Commands.DeleteMaterial;
using Voltiq.Application.Features.Materials.Commands.RegisterMaterial;
using Voltiq.Application.Features.Materials.Commands.UpdateMaterial;
using Voltiq.Application.Features.Materials.Queries.GetMaterialById;
using Voltiq.Application.Features.Materials.Queries.GetMaterials;
using Voltiq.Application.Mappings.Materials;

namespace Voltiq.API.Controllers.Materials;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/materials")]
public sealed class MaterialsController : BaseApiController
{
    /// <summary>Registers a new material for the authenticated user.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(MaterialResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterMaterialRequest request,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(request.ToCommand(), cancellationToken);

        return result.Match(
            material => CreatedAtAction(nameof(GetById), new { id = material.Id }, material),
            ToErrorResult);
    }

    /// <summary>Returns all materials belonging to the authenticated user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<MaterialResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetMaterialsQuery(), cancellationToken);

        return result.Match(Ok, ToErrorResult);
    }

    /// <summary>Returns a specific material by ID (must belong to the authenticated user).</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MaterialResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetMaterialByIdQuery(id), cancellationToken);

        return result.Match(Ok, ToErrorResult);
    }

    /// <summary>Updates a material (must belong to the authenticated user).</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateMaterialRequest request,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(request.ToCommand(id), cancellationToken);

        return result.Match(_ => NoContent(), ToErrorResult);
    }

    /// <summary>Deletes a material (must belong to the authenticated user).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new DeleteMaterialCommand(id), cancellationToken);

        return result.Match(_ => NoContent(), ToErrorResult);
    }
}

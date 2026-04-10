using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Voltiq.Application.Features.Budgets;
using Voltiq.Application.Features.Budgets.Commands.DeleteBudget;
using Voltiq.Application.Features.Budgets.Commands.RegisterBudget;
using Voltiq.Application.Features.Budgets.Queries.GetBudgetById;
using Voltiq.Application.Features.Budgets.Queries.GetBudgets;
using Voltiq.Application.Mappings.Budgets;

namespace Voltiq.API.Controllers.Budgets;

[ApiVersion("1.0")]
public sealed class BudgetsController : BaseApiController
{
    /// <summary>Creates a new budget with the specified client and items.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(BudgetDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterBudgetRequest request,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(request.ToCommand(), cancellationToken);

        return result.Match(
            budget => CreatedAtAction(nameof(GetById), new { id = budget.Id }, budget),
            ToErrorResult);
    }

    /// <summary>Returns all budgets belonging to the authenticated user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BudgetSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetBudgetsQuery(), cancellationToken);

        return result.Match(Ok, ToErrorResult);
    }

    /// <summary>Returns a specific budget by ID with full client and item details.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BudgetDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetBudgetByIdQuery(id), cancellationToken);

        return result.Match(Ok, ToErrorResult);
    }

    /// <summary>Deletes a budget (must belong to the authenticated user).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new DeleteBudgetCommand(id), cancellationToken);

        return result.Match(_ => NoContent(), ToErrorResult);
    }
}

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Voltiq.Application.Features.Budgets;
using Voltiq.Application.Features.Budgets.Commands.DeleteBudget;
using Voltiq.Application.Features.Budgets.Commands.RegisterBudget;
using Voltiq.Application.Features.Budgets.Commands.UpdateBudget;
using Voltiq.Application.Features.Budgets.Commands.FinalizeBudget;
using Voltiq.Application.Features.Budgets.Commands.ApproveBudget;
using Voltiq.Application.Features.Budgets.Commands.RejectBudget;
using Voltiq.Application.Features.Budgets.Commands.GenerateBudgetPdf;
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

    /// <summary>Updates a budget (must belong to the authenticated user).</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateBudgetRequest request,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(request.ToCommand(id), cancellationToken);

        return result.Match(_ => NoContent(), ToErrorResult);
    }

    /// <summary>Finalizes a budget, making it read-only.</summary>
    [HttpPut("{id:guid}/finalize")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Finalize(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new FinalizeBudgetCommand(id), cancellationToken);

        return result.Match(_ => Accepted(), ToErrorResult);
    }

    /// <summary>Generates/Regenerates the PDF for a finalized budget.</summary>
    [HttpPost("{id:guid}/generate-pdf")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GeneratePdf(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GenerateBudgetPdfCommand(id), cancellationToken);

        return result.Match(_ => Accepted(), ToErrorResult);
    }

    /// <summary>Approves a finalized budget.</summary>
    [HttpPut("{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new ApproveBudgetCommand(id), cancellationToken);

        return result.Match(_ => NoContent(), ToErrorResult);
    }

    /// <summary>Rejects a finalized budget.</summary>
    [HttpPut("{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new RejectBudgetCommand(id), cancellationToken);

        return result.Match(_ => NoContent(), ToErrorResult);
    }
}

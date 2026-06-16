using Billing.Application.Common.Invoices.Command.Cancel;
using Billing.Application.Common.Invoices.Command.Create;
using Billing.Application.Common.Invoices.Command.Pending;

using MediatR;

namespace Billing.Api.Endpoints;

public static class InvoiceEndpoints
{
    public static IEndpointRouteBuilder MapInvoiceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/invoices").WithTags("Invoices").RequireAuthorization();

        group.MapPost("/", async (IssueInvoiceCommand cmd, ISender sender) =>
        {
            var id = await sender.Send(cmd);
            return Results.Created($"/invoices/{id}", new { id });
        });

        group.MapPost("/{id:guid}/cancel", async (Guid id, CancelRequest body, ISender sender) =>
        {
            await sender.Send(new CancelInvoiceCommand(id, body.Reason));
            return Results.NoContent();
        });

        group.MapGet("/pending", async (ISender sender) =>
            Results.Ok(await sender.Send(new GetPendingInvoicesQuery())));

        return app;
    }
}

public sealed record CancelRequest(string Reason);

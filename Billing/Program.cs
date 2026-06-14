using Billing.Application;
using Billing.Application.Common.Invoices.Command.Cancel;
using Billing.Application.Common.Invoices.Command.Create;
using Billing.Application.Common.Invoices.Command.Pending;
using Billing.Infrastructure;

using FluentValidation;

using MediatR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHttpContextAccessor();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddAuthorization()

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.UseExceptionHandler(handler => handler.Run(async ctx =>
{
    var ex = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
    (ctx.Response.StatusCode, var message) = ex switch
    {
        ValidationException ve => (StatusCodes.Status400BadRequest,
            string.Join("; ", ve.Errors.Select(e => e.ErrorMessage))),
        Billing.Domain.Common.DomainException de => (StatusCodes.Status422UnprocessableEntity, de.Message),
        UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized."),
        _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
    };
    await ctx.Response.WriteAsJsonAsync(new { error = message });
}));

var invoices = app.MapGroup("/invoices").RequireAuthorization();

invoices.MapPost("/", async (IssueInvoiceCommand cmd, ISender sender) =>
{
    var id = await sender.Send(cmd);
    return Results.Created($"/invoices/{id}", new { id });
});

invoices.MapPost("/{id:guid}/cancel", async (Guid id, CancelRequest body, ISender sender) =>
{
    await sender.Send(new CancelInvoiceCommand(id, body.Reason));
    return Results.NoContent();
});

invoices.MapGet("/pending", async (ISender sender) =>
    Results.Ok(await sender.Send(new GetPendingInvoicesQuery())));

app.Run();




public sealed record CancelRequest(string Reason);
public partial class Program; 
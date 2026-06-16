using Billing.Api.Endpoints;
using Billing.Application;
using Billing.Infrastructure;

using FluentValidation;

using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Billing API", Version = "v1" });

    var bearer = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Paste your JWT here. Format: Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme }
    };

    c.AddSecurityDefinition(bearer.Reference.Id, bearer);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { [bearer] = Array.Empty<string>() });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Billing API v1");
        c.RoutePrefix = "swagger";
    });
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

app.MapAuthEndpoints();
app.MapInvoiceEndpoints();

app.Run();

public partial class Program;

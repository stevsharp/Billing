
using Billing.Application.Common.Abstractions;

using MediatR;

namespace Billing.Application.Common.Behaviors;

public sealed class TransactionBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork) : IPipelineBehavior<TRequest, TResponse> where TRequest : ITransactionalRequest
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ITransactionalRequest) 
            return await next();   // queries pass through

        TResponse response = default!;

        await unitOfWork.ExecuteTransactionalAsync(async () =>
        {
            response = await next();
        }, cancellationToken);

        return response;
    }
}
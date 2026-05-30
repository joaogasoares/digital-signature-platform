using DigitalSignature.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace DigitalSignature.Application.Common.Behaviors;

internal sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0) return await next();

        var errors = string.Join("; ", failures.Select(f => f.ErrorMessage));

        // Return Result.Failure if TResponse is Result<T> or Result
        if (typeof(TResponse).IsGenericType &&
            typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var resultType = typeof(TResponse).GetGenericArguments()[0];
            var failureMethod = typeof(Result<>).MakeGenericType(resultType)
                .GetMethod(nameof(Result<object>.Failure), [typeof(string)])!;
            return (TResponse)failureMethod.Invoke(null, [errors])!;
        }

        throw new ValidationException(failures);
    }
}

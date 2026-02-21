using ErrorOr;
using FluentValidation;
using MediatR;

namespace Relatio.Shared.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(request, cancellationToken)));

        var errors = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .Select(f => Error.Validation(
                code: f.PropertyName,
                description: f.ErrorMessage))
            .Distinct()
            .ToList();

        if (errors.Count != 0)
            return (TResponse)(dynamic)errors;

        return await next();
    }
}

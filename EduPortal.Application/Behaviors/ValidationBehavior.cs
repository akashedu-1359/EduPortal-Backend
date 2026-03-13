using FluentValidation;
using MediatR;
using EduPortal.Application.Common;

namespace EduPortal.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(e => e != null)
            .ToList();

        if (failures.Count != 0)
        {
            var errors = string.Join("; ", failures.Select(f => f.ErrorMessage));

            // If TResponse is a Result type, return failure
            if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var resultType = typeof(Result<>).MakeGenericType(typeof(TResponse).GetGenericArguments());
                var failureMethod = resultType.GetMethod(nameof(Result<object>.Failure), new[] { typeof(string), typeof(int) });
                var result = failureMethod!.Invoke(null, new object[] { errors, 400 });
                return (TResponse)result!;
            }

            throw new ValidationException(failures);
        }

        return await next();
    }
}

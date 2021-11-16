using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Behaviours
{
    public class RequestValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        private readonly ILogger _logger;
        public RequestValidationBehavior(IEnumerable<IValidator<TRequest>> validators, ILogger logger)
        {
            _logger = logger;
            _validators = validators;
        }
        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            if (!_validators.Any()) return await next();

            var context = new ValidationContext<TRequest>(request);
            var results = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = results.SelectMany(r => r.Errors)
            .Where(f => f != null)
            .GroupBy(x => x.PropertyName, x => x.ErrorMessage, (propertyName, errorMessage) => new
            {
                Key = propertyName,
                Values = errorMessage.Distinct().ToArray()
            })
            .ToDictionary(x => x.Key, x => x.Values);

            if (failures.Any())
            {
                _logger.LogInformation(failures.ToString());
                throw new Application.Exceptions.ValidationException((string)Resources.Reader.GetMessages()["ExceptionMessage"]["Validation"], failures);
            }

            return await next();
        }
    }
}
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
        private readonly IEnumerable<IValidator<TRequest>> validators;
        private readonly ILogger<RequestValidationBehavior<TRequest, TResponse>> logger;
        public RequestValidationBehavior(IEnumerable<IValidator<TRequest>> validators, ILogger<RequestValidationBehavior<TRequest, TResponse>> logger)
        {
            this.logger = logger;
            this.validators = validators;
        }
        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            if (!this.validators.Any()) return await next();

            var context = new ValidationContext<TRequest>(request);
            var results = await Task.WhenAll(this.validators.Select(v => v.ValidateAsync(context, cancellationToken)));

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
                foreach (var failure in failures)
                {
                    this.logger.LogInformation(string.Join(", ", failure.Value));
                }
                throw new Application.Exceptions.ValidationException((string)Resources.Reader.GetMessages()["ExceptionMessage"]["Validation"], failures);
            }

            return await next();
        }
    }
}
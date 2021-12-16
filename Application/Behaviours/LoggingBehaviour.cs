using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Behaviours
{
    public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> logger;

        public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
        {
            this.logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            try
            {
                this.logger.LogInformation($"Handling {typeof(TRequest).Name}");
                Type myType = request.GetType();
                IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());
                foreach (PropertyInfo prop in props)
                {
                    object propValue = prop.GetValue(request, null);
                    this.logger.LogInformation("{Property} : {@Value}", prop.Name, propValue);
                }
                var response = await next();

                this.logger.LogInformation($"Handled {typeof(TResponse).Name}");
                return response;

            }
            catch (System.Exception exception)
            {
                this.logger.LogError($"{Utf8Json.JsonSerializer.ToJsonString(exception)}");
                throw;
            }
        }
    }
}
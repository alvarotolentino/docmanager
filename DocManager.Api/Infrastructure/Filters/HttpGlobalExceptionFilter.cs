using System.Net;
using DocManager.Api.Infrastructure.Exceptions;
using DocManager.Api.Infrastructure.ActionResults;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace DocManager.Api.Infrastructure.Filters
{
    public class HttpGlobalExceptionFilter : IExceptionFilter
    {

        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment env;
        
        private readonly ILogger logger;

        public HttpGlobalExceptionFilter(IWebHostEnvironment env, ILogger logger)
        {
           this.env = env;
           this.logger = logger;
        }
        public void OnException(ExceptionContext context)
        {
           this.logger.LogError(new EventId(context.Exception.HResult), context.Exception, context.Exception.Message);
            if (context.Exception.GetType() == typeof(DomainException))
            {
                var problemDetails = new ValidationProblemDetails
                {
                    Instance = context.HttpContext.Request.Path,
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "Please refer to the errors property for additional details."
                };

                problemDetails.Errors.Add("DomainValidations", new string[] { context.Exception.Message.ToString() });
                context.Result = new BadRequestObjectResult(problemDetails);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            }
            else
            {
                var json = new JsonErrorResponse
                {
                    Messages = new[] { "An error ocurred." }
                };

                if (this.env.IsDevelopment())
                {
                    json.DeveloperMessage = context.Exception;
                }
                context.Result = new InternalServerErrorObjectResult(json);
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            }
        }
    }
    internal class JsonErrorResponse
    {
        public string[] Messages { get; set; }
        public object DeveloperMessage { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Utf8Json;

namespace DocManager.Api.Middleware
{
    public class ExceptionHandlingMiddleware : IMiddleware
    {
        private const string SERVERERROR = "Server Error"; 
        private const string CONTENTTYPE = "application/json"; 
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                await HandleException(context, exception);
            }
        }

        private static async Task HandleException(HttpContext context, Exception exception)
        {
            var code = GetStatusCode(exception);

            var response = new
            {
                title = GetTitle(exception),
                status = code,
                detail = exception.Message,
                errors = GetErrors(exception)
            };

            context.Response.ContentType = CONTENTTYPE;
            context.Response.StatusCode = code;
            await context.Response.WriteAsync(JsonSerializer.ToJsonString(response));

        }

        private static string GetTitle(Exception exception)
        {
            return exception switch
            {
                ValidationException ve => ve.Title,
                NotFoundException nfe => nfe.Title,
                _ => "Server Error"
            };
        }

        private static int GetStatusCode(Exception exception)
        {
            return exception switch
            {
                ValidationException _ => StatusCodes.Status400BadRequest,
                NotFoundException _ => StatusCodes.Status404NotFound,
                ApiException _ => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };
        }

        private static IReadOnlyDictionary<string, string[]> GetErrors(Exception exception)
        {
            IReadOnlyDictionary<string, string[]> errors = null;

            if (exception is ValidationException validationException)
            {
                errors = validationException.Errors;
            }

            return errors;

        }

    }
}
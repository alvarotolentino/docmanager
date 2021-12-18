using System.Net.Mime;
using DocManager.Api.Conventions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;

namespace DocManager.Api.Controllers
{
    [Authorize]
    [ApiConventionType(typeof(CustomApiConventions))]

    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]

    public abstract class BaseApiController : ControllerBase
    {

        private const int MAXRETRIES = 3;
        private IMediator mediator;
        protected IMediator Mediator => this.mediator ??= HttpContext.RequestServices.GetService<IMediator>();

        internal readonly AsyncRetryPolicy<IActionResult> retryPolicy;
        public BaseApiController()
        {
            retryPolicy = Policy<IActionResult>.Handle<Npgsql.NpgsqlException>().RetryAsync(retryCount: MAXRETRIES);
        }
    }
}
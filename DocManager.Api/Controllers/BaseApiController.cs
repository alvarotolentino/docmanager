using System.Net.Mime;
using DocManager.Api.Conventions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace DocManager.Api.Controllers
{
    [Authorize]
    [ApiConventionType(typeof(CustomApiConventions))]

    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]

    public abstract class BaseApiController : ControllerBase
    {
        private IMediator mediator;
        protected IMediator Mediator => this.mediator ??= HttpContext.RequestServices.GetService<IMediator>();
    }
}
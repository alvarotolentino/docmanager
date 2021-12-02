using System.Net.Mime;
using System.Threading.Tasks;
using Application.DTOs.Account;
using Application.Features.Account.Commands.AuthenticateUser;
using Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace DocManager.Api.Controllers
{
    [ApiController]
    [Route("api/")]
    public class SecurityController : ControllerBase
    {
        private IMediator mediator;
        protected IMediator Mediator => this.mediator ??= HttpContext.RequestServices.GetService<IMediator>();

        /// <summary>
        /// Login a user validating credentials
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The newly generated toke and refresh token</returns>

        [HttpPost("login")]
        public async Task<IActionResult> LogIn(AuthenticationRequest request)
        {
            var response = await Mediator.Send(new AuthenticateUser
            {
                email = request.Email,
                password = request.Password,
                ipaddress = GetIPAddress()
            });
            if (response.Succeeded)
                return Ok(response);

            if (response.Message.ToLower().Contains("unauthorize"))
                return Unauthorized(response);

            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        private string GetIPAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }
    }
}
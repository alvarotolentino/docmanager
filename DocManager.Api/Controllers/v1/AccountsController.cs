using System.Net.Mime;
using System.Threading.Tasks;
using Application.DTOs.Account;
using Application.Enums;
using Application.Features.Account.Commands.AddUserGroup;
using Application.Features.Account.Commands.AddUserRole;
using Application.Features.Account.Commands.RegisterAccount;
using Application.Features.Account.Queries.GetAccounts;
using Application.Interfaces;
using DocManager.Api.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DocManager.Api.Controllers
{
    [Authorize(UserRoles.Admin)]
    [ApiVersion("1.0")]

    public class AccountsController : BaseApiController
    {

        /// <summary>
        /// Register a new user account.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>The newly created User Id</returns>
        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] RegisterAccount request)
        {
            var response = await Mediator.Send(request);
            if (response.Succeeded)
                return StatusCode(StatusCodes.Status201Created, response);

            // TODO: Validate 400 error or Validation Exception 
            return Ok(response);

        }

        /// <summary>
        /// Assigns a specified user to a specified group
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="groupid"></param>
        /// <returns></returns>

        [HttpPost("{userId}/group/{groupId}")]
        public async Task<IActionResult> AddGroup([FromRoute] long userid, [FromRoute] long groupid)
        {
            return Ok(await Mediator.Send(new AddUserGroup { userid = userid, groupid = groupid }));
        }

        /// <summary>
        /// Assings a specified role
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="roleid"></param>
        /// <returns></returns>
        [HttpPost("{userId}/role/{roleid}")]
        public async Task<IActionResult> AddRole([FromRoute] long userid, [FromRoute] long roleid)
        {
            return Ok(await Mediator.Send(new AddUserRole { userid = userid, roleid = roleid }));
        }


        /// <summary>
        /// Get all accoutns paginated
        /// </summary>
        /// <param name="pagination">Page size and page number</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAccounts([FromQuery] GetAllAccountsParameter pagination)
        {
            return Ok(await Mediator.Send(new GetAllAccountsQuery { PageSize = pagination.pagesize, PageNumber = pagination.pagenumber }));
        }


    }
}
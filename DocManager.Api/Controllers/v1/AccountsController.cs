using System.Net.Mime;
using System.Threading.Tasks;
using Application.DTOs.Account;
using Application.Enums;
using Application.Features.Account.Commands.AddUserGroup;
using Application.Features.Account.Commands.AddUserRole;
using Application.Features.Account.Commands.RegisterAccount;
using Application.Interfaces;
using DocManager.Api.Attributes;
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
        /// <param name="command"></param>
        /// <returns>The newly created User Id</returns>
        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] RegisterAccountCommand command)
        {
            return Ok(await Mediator.Send(command));
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
            return Ok(await Mediator.Send(new AddUserGroupCommand { userid = userid, groupid = groupid }));
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
            return Ok(await Mediator.Send(new AddUserRoleCommand { userid = userid, roleid = roleid }));
        }


    }
}
using System.Threading.Tasks;
using Application.Enums;
using Application.Features.Groups.Commands.DeleteGroup;
using Application.Features.Groups.Commands.RegisterGroup;
using Application.Features.Groups.Commands.UpdateGroup;
using Application.Features.Groups.Queries.GetGroupById;
using Application.Features.Groups.Queries.GetGroups;
using DocManager.Api.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace DocManager.Api.Controllers.v1
{
    [Authorize(UserRoles.Admin)]
    [ApiVersion("1.0")]
    public class GroupsController : BaseApiController
    {

        /// <summary>
        /// Creates a new group
        /// </summary>
        /// <param name="command"></param>
        /// <returns>The newly created group id</returns>  
        [HttpPost]
        public async Task<IActionResult> CreateGroup([FromBody] RegisterGroupCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGroup(long id, UpdateGroupCommand command)
        {
            command.id = id;
            return Ok(await Mediator.Send(command));
        }

        /// <summary>
        /// Deletes a group
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGroup(long id)
        {
            return Ok(await Mediator.Send(new DeleteGroupCommand { id = id }));
        }

        /// <summary>
        /// Gets a group by its id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>A group</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGroup(long id)
        {
            return Ok(await Mediator.Send(new GetGroupByIdQuery { Id = id }));
        }

        /// <summary>
        /// Gets all groups
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetGroups([FromQuery] GetAllGroupsParameter pagination)
        {
            return Ok(await Mediator.Send(new GetGroupsQuery() { PageSize = pagination.pagesize, PageNumber = pagination.pagenumber }));
        }

    }
}
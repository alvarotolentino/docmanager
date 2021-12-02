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
        /// <param name="request"></param>
        /// <returns>The newly created group id</returns>  
        [HttpPost]
        public async Task<IActionResult> CreateGroup([FromBody] RegisterGroup request)
        {
            return Ok(await Mediator.Send(request));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGroup(int id, UpdateGroup request)
        {
            request.Id = id;
            return Ok(await Mediator.Send(request));
        }

        /// <summary>
        /// Deletes a group
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGroup(int id)
        {
            return Ok(await Mediator.Send(new DeleteGroup { Id = id }));
        }

        /// <summary>
        /// Gets a group by its id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>A group</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGroup(int id)
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
            return Ok(await Mediator.Send(new GetGroupsQuery() { PageSize = pagination.PageSize, PageNumber = pagination.PageNumber }));
        }

    }
}
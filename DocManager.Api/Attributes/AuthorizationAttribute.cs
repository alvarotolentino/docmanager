using System;
using System.Collections.Generic;
using System.Linq;
using Application.Enums;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DocManager.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly IList<UserRoles> roles;

        public AuthorizeAttribute(params UserRoles[] roles)
        {
            this.roles = roles ?? new UserRoles[] { };
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
            if (allowAnonymous)
                return;

            var userRoles = (IEnumerable<string>)context.HttpContext.Items["Roles"];

            if (userRoles == null || (this.roles.Any() && !this.roles.Select(x => x.ToString()).Intersect(userRoles).Any()))
            {
                context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
            }
        }
    }
}
using System.Security.Claims;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace DocManager.Api.Services
{
    public class AuthenticatedUserService : IAuthenticatedUserService
    {
        public long? UserId { get; }

        public AuthenticatedUserService(IHttpContextAccessor httpContextAccessor)
        {
            UserId = ((User)httpContextAccessor.HttpContext?.Items?["User"])?.Id ;
        }
    }
}